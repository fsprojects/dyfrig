﻿[<AutoOpen>]
module Freya.Machine.Types

open System
open Freya.Core

(* Types *)

type MachineUnary =
    Core<unit>

type MachineBinary =
    Core<bool>

(* Configuration *)

type MachineConfiguration =
    { Configuration: Map<string, obj> }

    static member ConfigurationLens =
        (fun x -> x.Configuration), (fun d x -> { x with Configuration = d })

type MachineConfigurationMetadata =
    { Configurable: bool
      Configured: bool }

(* Graph

   Types used for defining elements of graphs, common to multiple
   graph definition types. Node and edge references are defined with
   simple string keys, relying on regular comparison. *)

type MachineNodeRef =
    | Start
    | Finish
    | Node of string

type MachineEdgeRef =
    | Edge of MachineNodeRef * MachineNodeRef

(* Types

   Types defining an execution graph and supporting metadata. *)

type MachineExecutionGraph =
    { Nodes: Map<MachineNodeRef, MachineExecutionNode option> }

and MachineExecutionNode =
    | ExecutionUnary of MachineExecutionUnary
    | ExecutionBinary of MachineExecutionBinary

and MachineExecutionUnary =
    { Unary: MachineUnary
      Configuration: MachineConfigurationMetadata
      Next: MachineNodeRef }

and MachineExecutionBinary =
    { Binary: MachineBinary
      Configuration: MachineConfigurationMetadata
      True: MachineNodeRef
      False: MachineNodeRef }

(* Definition

   Types representing the definition used to produce an execution
   graph, using a standard nodes and edges representation. Operations on
   the graph are represented as being able to succeed, as a mapping of
   definition graph -> definition graph, or fail as an error. *)

type MachineDefinitionGraph =
    { Nodes: Map<MachineNodeRef, MachineDefinitionNode option>
      Edges: Map<MachineEdgeRef, MachineDefinitionEdge> }

    static member NodesLens =
        (fun x -> x.Nodes), (fun n x -> { x with MachineDefinitionGraph.Nodes = n })

    static member EdgesLens =
        (fun x -> x.Edges), (fun e x -> { x with MachineDefinitionGraph.Edges = e })

and MachineDefinitionNode =
    | Unary of MachineDefinitionUnary
    | Binary of MachineDefinitionBinary

and MachineDefinitionUnary =
    MachineConfiguration -> MachineConfigurationMetadata * MachineUnary

and MachineDefinitionBinary =
    MachineConfiguration -> MachineConfigurationMetadata * MachineBinary

and MachineDefinitionEdge =
    | Value of bool option

type MachineDefinitionOperation =
    MachineDefinitionGraph -> Choice<MachineDefinitionGraph, string>

(* Extension *)

[<CustomEquality; CustomComparison>]
type MachineExtension =
    { Name: string
      Dependencies: Set<string>
      Operations: MachineDefinitionOperation list }

    static member private Comparable (x: MachineExtension) =
        x.Name, x.Dependencies

    override x.Equals y =
        equalsOn MachineExtension.Comparable x y

    override x.GetHashCode () =
        hashOn MachineExtension.Comparable x

    interface IComparable with

        member x.CompareTo y =
            compareOn MachineExtension.Comparable x y

(* Computation Expression *)

type Machine =
    MachineDefinition -> unit * MachineDefinition

and MachineDefinition =
    { Configuration: MachineConfiguration
      Extensions: Set<MachineExtension> }

    static member ConfigurationLens =
        (fun x -> x.Configuration), (fun d x -> { x with MachineDefinition.Configuration = d })

    static member ExtensionsLens =
        (fun x -> x.Extensions), (fun e x -> { x with Extensions = e })