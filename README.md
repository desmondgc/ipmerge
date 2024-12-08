# IPMerge
Turn this:
```text
192.168.100.44
192.168.200.17
192.168.200.155
192.168.200.156
192.168.100.2
192.168.100.3
192.168.100.5
...
...
... and so on
```

into this:
```text
192.168.100.0/26
192.168.100.65
192.168.100.68
192.168.200.17
192.168.200.128/25
```

by finding and combining _mostly complete_ subnets.

## Usage
```
ipmerge <path> [prefix_length]
```
