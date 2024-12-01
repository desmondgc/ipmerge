// For more information see https://aka.ms/fsharp-console-apps

open System
open System.IO
open System.Linq
open System.Net

printfn "Hello from F#"

type IPAddressOrCidr =
    | IP of IPAddress
    | Cidr of IPNetwork

let parseIPAddress (str: string) =
    if str[^3] = '/' || str[^2] = '/' then
        Cidr (IPNetwork.Parse(str))
    else
        IP (IPAddress.Parse(str))
        
let readIPAddresses (path: string) =
    seq {
        use reader = new StreamReader(path)
        while not reader.EndOfStream do
            yield parseIPAddress (reader.ReadLine())
    }
    
let subnetSize (prefixLength: int) =
    if prefixLength < 0 || prefixLength > 31 then
        invalidArg (nameof prefixLength) $"Value passed in was %d{prefixLength}."
    else
        1 <<< (32 - prefixLength)

let ipToInt (ip: IPAddress) =
    BitConverter.ToUInt32(Array.rev (ip.GetAddressBytes()), 0)
    
let intToIP (ip: uint32) =
    IPAddress(Array.rev (BitConverter.GetBytes(ip)))
    
let getMask prefixLength =
    0xFFFFFFFFu <<< (32 - prefixLength)
    
let getNetAddress ip prefixLength =
    let masked = ipToInt ip &&& getMask prefixLength
    IPNetwork(intToIP masked, prefixLength)

let justIPs = function
    | IP ip -> Some ip
    | _ -> None

let findMostlyCompleteSubnets prefixLength (ipsOrCidrs: IPAddressOrCidr seq) =    
    let completenessThreshold =
        (subnetSize prefixLength) + prefixLength - 32
    
    query {
        for ip in (Seq.choose justIPs ipsOrCidrs) do
        groupBy (getNetAddress ip prefixLength) into g
        where (g.Count() >= completenessThreshold)
        select g.Key
    }
    
let supernet (net: IPNetwork) =
    getNetAddress net.BaseAddress (net.PrefixLength - 1)
    
let isSuper (net1: IPNetwork) (net2: IPNetwork) =
    net1.PrefixLength < net2.PrefixLength
    && (getNetAddress net2.BaseAddress net1.PrefixLength) = net1
    
let canCombine (net1: IPNetwork) (net2: IPNetwork) =
    net1.PrefixLength = net2.PrefixLength
    && supernet net1 = supernet net2
    
let collapse (nets: IPNetwork seq) = 
    let rec foldNet acc net =
        match acc with
        | [] -> [ net ]
        | x :: xs ->
            if canCombine net x then
                // Combine the current and previous network and continue backtracking
                foldNet xs (supernet net)
            else if isSuper x net then
                // Skip the current network if it is encompassed by the previous one
                x :: xs
            else
                net :: x :: xs

    nets
    |> Seq.sortBy (fun net -> (ipToInt net.BaseAddress, net.PrefixLength))
    |> Seq.fold foldNet []
    |> Seq.rev
    
let goForIt path prefixLength =
    let ips = Seq.cache (readIPAddresses path)
    
    let nets =
        ips
        |> findMostlyCompleteSubnets prefixLength
        |> collapse
        |> Seq.toList
    
    seq {
        for ip in ips |> Seq.choose justIPs do
            match nets |> List.tryFind (_.Contains(ip)) with
            | Some net -> yield Cidr net
            | None -> yield IP ip
    }
    |> Seq.distinct
    |> Seq.iter (printfn "%A")
    
[<EntryPoint>]
let main argv =
    match argv with
    | [| path |] ->
        goForIt path 29
        0
    | [| path; prefixLength |] ->
        goForIt path (Int32.Parse(prefixLength))
        0
    | _ ->
        printfn "Usage: ipmerge <path> [length]"
        1