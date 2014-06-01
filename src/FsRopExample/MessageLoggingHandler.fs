namespace FsRopExample

open System
open System.Diagnostics
open System.Net.Http
open System.Threading

/// Logging code 
type MessageLoggingHandler() =
    inherit MessageProcessingHandler()

    override this.ProcessRequest(request:HttpRequestMessage , _:CancellationToken) =
        let correlationId = sprintf "%i%i" DateTime.Now.Ticks Thread.CurrentThread.ManagedThreadId
        let requestInfo = sprintf "%O %O" request.Method  request.RequestUri
        let message = request.Content.ReadAsStringAsync().Result
        Debug.WriteLine("[HTTP]Request: {1}\r\n[HTTP]{2}\r\n\r\n", correlationId, requestInfo, message)
        request

    override this.ProcessResponse(response:HttpResponseMessage, _:CancellationToken) =
        let correlationId = sprintf "%i%i" DateTime.Now.Ticks Thread.CurrentThread.ManagedThreadId
        let requestInfo = sprintf "%O %O" response.RequestMessage.Method  response.RequestMessage.RequestUri
        let message = if response.Content <> null then response.Content.ReadAsStringAsync().Result else "[no body]"
        Debug.WriteLine("[HTTP]Response: {1}\r\n[HTTP]{2}\r\n\r\n", correlationId, requestInfo, message)
        response
