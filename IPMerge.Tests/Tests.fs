module Tests

open System.Net
open Xunit
open Program

[<Fact>]
let ``Mostly complete subnet`` () =
    let ips = [
        IPAddress.Parse("10.0.0.1")
        IPAddress.Parse("10.0.0.2")
        IPAddress.Parse("10.0.0.3")
        IPAddress.Parse("10.0.0.4")
        IPAddress.Parse("10.0.0.5")
        //IPAddress.Parse("10.0.0.6")
    ]
    let expected = [
        IPNetwork.Parse("10.0.0.0/29")
    ]
    let actual = findMostlyCompleteSubnets 29 ips
    Assert.Equal(expected, actual)
    
[<Fact>]
let ``Incomplete subnet`` () =
    let ips = [
        IPAddress.Parse("10.0.0.1")
        //IPAddress.Parse("10.0.0.2")
        //IPAddress.Parse("10.0.0.3")
        IPAddress.Parse("10.0.0.4")
        IPAddress.Parse("10.0.0.5")
        //IPAddress.Parse("10.0.0.6")
    ]
    let actual = findMostlyCompleteSubnets 29 ips
    Assert.Empty(actual)
    
[<Fact>]
let ``Two mostly complete subnets`` () =
    let ips = [
        IPAddress.Parse("10.0.0.1")
        IPAddress.Parse("10.0.0.2")
        IPAddress.Parse("10.0.0.3")
        IPAddress.Parse("10.0.0.4")
        IPAddress.Parse("10.0.0.5")
        //IPAddress.Parse("10.0.0.6")
        IPAddress.Parse("192.168.1.65")
        IPAddress.Parse("192.168.1.66")
        IPAddress.Parse("192.168.1.67")
        IPAddress.Parse("192.168.1.68")
        //IPAddress.Parse("192.168.1.69")
        IPAddress.Parse("192.168.1.70")
    ]
    let expected = [
        IPNetwork.Parse("10.0.0.0/29")
        IPNetwork.Parse("192.168.1.64/29")
    ]
    let actual = findMostlyCompleteSubnets 29 ips
    Assert.Equal(expected, actual)
    
[<Fact>]
let ``Mostly complete /28 subnet`` () =
    let ips = [
        IPAddress.Parse("10.0.0.1")
        IPAddress.Parse("10.0.0.2")
        IPAddress.Parse("10.0.0.3")
        IPAddress.Parse("10.0.0.4")
        IPAddress.Parse("10.0.0.5")
        IPAddress.Parse("10.0.0.6")
        IPAddress.Parse("10.0.0.7")
        IPAddress.Parse("10.0.0.8")
        IPAddress.Parse("10.0.0.9")
        //IPAddress.Parse("10.0.0.10")
        //IPAddress.Parse("10.0.0.11")
        IPAddress.Parse("10.0.0.12")
        IPAddress.Parse("10.0.0.13")
        IPAddress.Parse("10.0.0.14")
    ]
    let expected = [
        IPNetwork.Parse("10.0.0.0/28")
    ]
    let actual = findMostlyCompleteSubnets 28 ips
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
let ``Combine overlapping subnets`` () =
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

[<EntryPoint>]
let main _ = 0
