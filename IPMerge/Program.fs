// For more information see https://aka.ms/fsharp-console-apps

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Net

printfn "Hello from F#"

type IPAddressOrCIDR =
    | IP of IPAddress
    | CIDR of IPNetwork

let parseIPAddress (line: string) =
    if line.Contains("/") then
        CIDR (IPNetwork.Parse(line))
    else
        IP (IPAddress.Parse(line))
        
let readIPAddresses (path: string) =
    seq {
        use reader = new StreamReader(path)
        while not reader.EndOfStream do
            yield parseIPAddress (reader.ReadLine())
    }
    
let routableAddresses (prefixLength: int) =
    if prefixLength < 0 || prefixLength > 31 then
        invalidArg (nameof prefixLength) $"Value passed in was %d{prefixLength}."
    else
        (1 <<< (32 - prefixLength)) - 2

let ipToInt (ip: IPAddress) : uint32 =
    BitConverter.ToUInt32(Array.rev (ip.GetAddressBytes()), 0)
    
let intToIP (ip: uint32) : IPAddress =
    IPAddress(Array.rev (BitConverter.GetBytes(ip)))
    
let getMask prefixLength =
    0xFFFFFFFFu <<< (32 - prefixLength)
    
let getNetAddress ip prefixLength =
    ipToInt ip &&& (getMask prefixLength)
    |> intToIP

let findMostlyCompleteSubnets prefixLength (ipsOrCidrs: IPAddressOrCIDR seq) =    
    let justIPs =
        ipsOrCidrs
        |> Seq.choose (function
            | IP ip -> Some ip
            | _ -> None)
        
    let completenessThreshold = (routableAddresses prefixLength) - (30 - prefixLength)
    
    query {
        for ip in justIPs do
        groupBy (IPNetwork((getNetAddress ip prefixLength), prefixLength)) into g
        where (g.Count() >= completenessThreshold)
        select g.Key
    }
    
let supernet (net: IPNetwork) =
    let prefix = net.PrefixLength - 1
    IPNetwork(getNetAddress net.BaseAddress prefix, prefix)
    
let canCombine (net1: IPNetwork) (net2: IPNetwork) =
    net1.PrefixLength = net2.PrefixLength
    && supernet net1 = supernet net2
    
let collapse (nets: IPNetwork list) =
    let sortedNetworks = List.sortBy (fun (net: IPNetwork) -> (ipToInt net.BaseAddress, net.PrefixLength)) nets
    
    let result = List<IPNetwork>()
    let mutable i = 0
    
    while i < sortedNetworks.Length do
        let mutable current = sortedNetworks[i]
        let mutable j = i + 1

        while j < sortedNetworks.Length && canCombine current sortedNetworks[j] do
            current <- supernet current
            j <- j + 1

        // Backtrack to merge with previous networks if possible
        while result.Count > 0 && canCombine result[result.Count - 1] current do
            current <- supernet result[result.Count - 1]
            result.RemoveAt(result.Count - 1)

        result.Add(current)
        i <- j

    List.ofSeq result
    