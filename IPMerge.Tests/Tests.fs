module Tests

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
    let expected = [
        IPNetwork.Parse("10.0.0.0/29")
    ]
    let actual = findMostlyCompleteSubnets 29 <| Seq.map parseIPAddress ips
    Assert.Equal(expected, actual)
    
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
    let actual = findMostlyCompleteSubnets 29 <| Seq.map parseIPAddress ips
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
    let actual = findMostlyCompleteSubnets 29 <| Seq.map parseIPAddress ips
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
    let expected = [
        IPNetwork.Parse("10.0.0.0/28")
    ]
    let actual = findMostlyCompleteSubnets 28 <| Seq.map parseIPAddress ips
    Assert.Equal(expected, actual)
    
[<Fact>]
let ``supernet`` () =
    let expected = IPNetwork.Parse("255.255.255.252/30")
    let actual = supernet (IPNetwork.Parse("255.255.255.254/31"))
    Assert.Equal(expected, actual)
    
[<Fact>]
let ``Combine subnets simple`` () =
    let nets = [
        IPNetwork.Parse("10.0.0.0/25")
        IPNetwork.Parse("10.0.0.128/26")
        IPNetwork.Parse("10.0.0.192/27")
        IPNetwork.Parse("10.0.0.224/27")
    ]
    let expected = [
        IPNetwork.Parse("10.0.0.0/24")
    ]
    let actual = collapse nets
    Assert.Equal(expected, actual)
    
[<Fact>]
let ``Combine subnets complex`` () =
    let nets = [
        IPNetwork.Parse("10.0.0.0/24")
        IPNetwork.Parse("10.0.1.0/24")
        IPNetwork.Parse("10.0.2.0/23")
        IPNetwork.Parse("10.0.4.0/26")
        IPNetwork.Parse("10.0.4.64/26")
        IPNetwork.Parse("10.0.5.0/24")
        IPNetwork.Parse("10.0.6.0/24")
        IPNetwork.Parse("10.0.7.0/26")
        IPNetwork.Parse("10.0.7.64/26")
        IPNetwork.Parse("10.0.7.128/25")
        IPNetwork.Parse("10.0.8.128/26")
        IPNetwork.Parse("10.0.8.192/26")
    ]
    let expected = [
        IPNetwork.Parse("10.0.0.0/22")
        IPNetwork.Parse("10.0.4.0/25")
        IPNetwork.Parse("10.0.5.0/24")
        IPNetwork.Parse("10.0.6.0/23")
        IPNetwork.Parse("10.0.8.128/25")
    ]
    let actual = collapse nets
    Assert.Equal(expected, actual)

[<Fact>]
let ``Combine subnets with overlap`` () =
    let nets = [
        IPNetwork.Parse("10.0.0.0/23")
        IPNetwork.Parse("10.0.1.0/24")
        IPNetwork.Parse("10.0.2.0/24")
        IPNetwork.Parse("10.0.4.0/26")
    ]
    let expected = [
        IPNetwork.Parse("10.0.0.0/23")
        IPNetwork.Parse("10.0.2.0/24")
        IPNetwork.Parse("10.0.4.0/26")
    ]
    let actual = collapse nets
    Assert.Equal(expected , actual)
    