module FsRopExample.DomainPrimitiveTypes

open System
open FsRopExample.Rop

// demonstrates use of opaque types

type StringError =
    | Missing
    | MustNotBeLongerThan of int
    | DoesntMatchPattern of string

type IntegerError =
    | Missing
    | MustBePositiveInteger

module String10 =
    type T
    val create : string -> RopResult<T,StringError>
    val apply : (string -> 'a) -> T -> 'a

module String20 =
    type T
    val create : string -> RopResult<T,StringError>
    val apply : (string -> 'a) -> T -> 'a

module EmailAddress =
    type T
    val create : string -> RopResult<T,StringError>
    val apply : (string -> 'a) -> T -> 'a

module CustomerId =
    type T
    val create : int -> RopResult<T,IntegerError>
    // Here's an example using nullable just to demonstrate how to use it as input
    val createFromNullable : Nullable<int> -> RopResult<T,IntegerError>
    val apply : (int -> 'a) -> T -> 'a

