module CommonTypes

type String10 = String10 of string
type String20 = String20 of string
type EmailAddress = EmailAddress of String20


let createString10 (s:string) =
    match s with
    | null -> None
    | _ when s.Length > 10 -> None
    | _ -> String10 s |> Some
    
let createString20 (s:string) =
    match s with
    | null -> None
    | _ when s.Length > 20 -> None
    | _ -> String20 s |> Some

let createEmail (s:string) =
    match createString20 s with
    | None -> None
    | Some s' when s.Contains("@") -> 
        EmailAddress s' |> Some
    | _ -> None
