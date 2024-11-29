module Tests

open System
open System.Linq
open System.Net
open Program
open Xunit

[<Fact>]
let ``Mostly complete subnet`` () =
    let ips = [
        "10.0.0.1"
        "10.0.0.2"
        "10.0.0.3"
        "10.0.0.4"
        "10.0.0.5"
        //"10.0.0.6"
    ]
    let actual = Seq.map parseIPAddress ips
                 |> findMostlyCompleteSubnets 29
    Assert.Equal(IPNetwork.Parse("10.0.0.0/29"), actual.Single())
    
[<Fact>]
let ``Incomplete subnet`` () =
    let ips = [
        "10.0.0.1"
        //"10.0.0.2"
        //"10.0.0.3"
        "10.0.0.4"
        "10.0.0.5"
        //"10.0.0.6"
    ]
    let actual = Seq.map parseIPAddress ips
                 |> findMostlyCompleteSubnets 29
    Assert.Empty(actual)
    
[<Fact>]
let ``Two mostly complete subnets`` () =
    let ips = [
        "10.0.0.1"
        "10.0.0.2"
        "10.0.0.3"
        "10.0.0.4"
        "10.0.0.5"
        //"10.0.0.6"
        "192.168.1.65"
        "192.168.1.66"
        "192.168.1.67"
        "192.168.1.68"
        //"192.168.1.69"
        "192.168.1.70"
        "127.0.0.1"
    ]
    let expected = [
        IPNetwork.Parse("10.0.0.0/29")
        IPNetwork.Parse("192.168.1.64/29")
    ]
    let actual = Seq.map parseIPAddress ips
                 |> findMostlyCompleteSubnets 29
    Assert.Equal(expected, actual)
    
[<Fact>]
let ``Mostly complete /28 subnet`` () =
    let ips = [
        "10.0.0.1"
        "10.0.0.2"
        "10.0.0.3"
        "10.0.0.4"
        "10.0.0.5"
        "10.0.0.6"
        "10.0.0.7"
        "10.0.0.8"
        "10.0.0.9"
        //"10.0.0.10"
        //"10.0.0.11"
        "10.0.0.12"
        "10.0.0.13"
        "10.0.0.14"
    ]
    let actual = Seq.map parseIPAddress ips
                 |> findMostlyCompleteSubnets 28
    Assert.Equal(IPNetwork.Parse("10.0.0.0/28"), actual.Single())
    
[<Fact>]
let ``Combine subnets simple`` () =
    let nets = [
        IPNetwork.Parse("10.0.0.0/25")
        IPNetwork.Parse("10.0.0.128/26")
        IPNetwork.Parse("10.0.0.192/27")
        IPNetwork.Parse("10.0.0.224/27")
    ]
    let actual = collapse nets
    Assert.Equal<IPNetwork list>([ IPNetwork.Parse("10.0.0.0/24") ], actual)
    
[<Fact>]
let ``Combine subnets complex`` () =
    let nets = [
        IPNetwork.Parse("10.0.0.0/25")
        IPNetwork.Parse("10.0.0.128/26")
        IPNetwork.Parse("10.0.0.192/27")
        IPNetwork.Parse("10.0.0.224/27")
        IPNetwork.Parse("10.0.1.0/26")
        IPNetwork.Parse("10.0.2.0/24")
    ]
    let actual = collapse nets
    Assert.Equal<IPNetwork list>([
        IPNetwork.Parse("10.0.0.0/24")
        IPNetwork.Parse("10.0.1.0/26")
        IPNetwork.Parse("10.0.2.0/24")
    ], actual)
    
[<Fact>]
let ``supernet`` () =
    let actual = supernet (IPNetwork.Parse("255.255.255.254/31"))
    Assert.Equal(IPNetwork.Parse("255.255.255.252/30"), actual)