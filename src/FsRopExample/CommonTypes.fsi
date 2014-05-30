module CommonTypes

// demonstrates use of opaque types
type String10 
type String20 
type EmailAddress 

// only these constructors can make these types
val createString10 : string -> String10 option
val createString20 : string -> String20 option
val createEmail: string -> EmailAddress option
