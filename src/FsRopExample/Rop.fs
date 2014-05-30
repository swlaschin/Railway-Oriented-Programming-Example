module Rop

type Error =
    | FirstNameMissing
    | FirstNameTooLong
    | LastNameMissing
    | LastNameTooLong
    | EmailMissing
    | EmailNotValidFormat
    | EmailTooLong
    | DatabaseError of string

type RopResult<'T> =
    | Success of 'T
    | Failure of Error list


let bind f result =
    match result with
    | Success s -> f s
    | Failure errs -> Failure errs 

let map f result =
    match result with
    | Success s -> f s |> Success 
    | Failure errs -> Failure errs 

let succeed v =
    Success v

let fail err =
    Failure [err]

let apply f result =
    match f,result with
    | Success f, Success s -> f s |> Success 
    | Failure errs, Success _ 
    | Success _, Failure errs -> Failure errs 
    | Failure errs1, Failure errs2 -> Failure (errs1 @ errs2)

let (<!) = map
let (<*>) = apply

let add result1 result2 =
    match result1,result2  with
    | Success s1, Success s2 when s1=s2-> Success s1
    | Success s1, Success s2 -> failwith "both results must have the same value"
    | Failure errs, Success _ 
    | Success _, Failure errs -> Failure errs 
    | Failure errs1, Failure errs2 -> Failure (errs1 @ errs2)
