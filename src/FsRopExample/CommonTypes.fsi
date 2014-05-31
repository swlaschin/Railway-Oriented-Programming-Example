module FsRopExample.CommonTypes

// demonstrates use of opaque types

module String10 =
    type T
    val create : string -> T option
    val apply : (string -> 'a) -> T -> 'a

module String20 =
    type T
    val create : string -> T option
    val apply : (string -> 'a) -> T -> 'a

module EmailAddress =
    type T
    val create : string -> T option
    val apply : (string -> 'a) -> T -> 'a

