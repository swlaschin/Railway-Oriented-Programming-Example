module FsRopExample.CommonTypes


module String10 =
    type T = String10 of string

    let create (s:string) =
        match s with
        | null -> None
        | _ when s.Length > 10 -> None
        | _ -> String10 s |> Some

    let apply f (String10 s) =
        f s

module String20 =
    type T = String20 of string

    let create (s:string) =
        match s with
        | null -> None
        | _ when s.Length > 20 -> None
        | _ -> String20 s |> Some

    let apply f (String20 s) =
        f s

module EmailAddress =
    type T = EmailAddress of String20.T

    let create (s:string) =
        match String20.create s with
        | None -> None
        | Some s' when s.Contains("@") -> 
            EmailAddress s' |> Some
        | _ -> None

    let apply f (EmailAddress s20) =
        let s = s20 |> String20.apply id
        f s

