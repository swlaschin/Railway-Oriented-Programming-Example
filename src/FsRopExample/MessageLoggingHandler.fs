namespace FsRopExample

open System
open System.Diagnostics
open System.Net.Http
open System.Threading

/// Logging code 
type MessageLoggingHandler() =
    inherit MessageProcessingHandler()

    override this.ProcessRequest(request:HttpRequestMessage , cancellationToken:CancellationToken) =
        let correlationId = sprintf "%i%i" DateTime.Now.Ticks Thread.CurrentThread.ManagedThreadId
        let requestInfo = sprintf "%O %O" request.Method  request.RequestUri
        let message = request.Content.ReadAsStringAsync().Result
        Debug.WriteLine("{0} - Request: {1}\r\n{2}", correlationId, requestInfo, message)
        request

    override this.ProcessResponse(response:HttpResponseMessage, cancellationToken:CancellationToken) =
        let correlationId = sprintf "%i%i" DateTime.Now.Ticks Thread.CurrentThread.ManagedThreadId
        let requestInfo = sprintf "%O %O" response.RequestMessage.Method  response.RequestMessage.RequestUri
        let message = if response.Content <> null then response.Content.ReadAsStringAsync().Result else "[no body]"
        Debug.WriteLine("{0} - Response: {1}\r\n{2}", correlationId, requestInfo, message)
        response
