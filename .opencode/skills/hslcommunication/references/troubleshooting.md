# TCP/UDP client connection troubleshooting

Always inspect the `Message` returned by `ConnectServer()` or by read/write
methods. Match the message to the most likely cause before changing code.

Whether to ask the user to confirm connection values after a failure depends
on the task:

- For an interactive sample or a one-off connection test, asking the user to
  confirm or update the values is appropriate.
- For configurable or complete-project code, do not automatically add an
  interactive confirmation step. Return or log the `OperateResult`, update the
  connection status, and follow the configured retry/backoff policy. Ask the
  current user again only when they explicitly want to diagnose the failure or
  update the configuration source.

This section applies to the supported TCP/UDP clients:

- `ModbusTcpNet`, `ModbusUdpNet`, `ModbusRtuOverTcp`, `ModbusAsciiOverTcp`
- `MelsecMcNet`, `MelsecMcUdp`, `MelsecMcAsciiNet`, `MelsecMcAsciiUdp`
- `MelsecA1ENet`, `MelsecA1EAsciiNet`
- `MelsecA3CNetOverTcp`, `MelsecA4CNetOverTcp`, `MelsecFxLinksOverTcp`, `MelsecFxSerialOverTcp`
- `OmronFinsNet`, `OmronFinsUdp`
- `OmronHostLinkOverTcp`, `OmronHostLinkCModeOverTcp`
- `OmronCipNet`, `OmronConnectedCipNet`
- `AllenBradleyNet`, `AllenBradleyConnectedCipNet`
- `AllenBradleyMicroCip`
- `AllenBradleyPcccNet`
- `AllenBradleySLCNet`
- `BeckhoffAdsNet`
- `CimonHmiProtocol`
- `DeltaSerialOverTcp`
- `DeltaSerialAsciiOverTcp`
- `DeltaTcpNet`
- `InovanceSerialOverTcp`
- `InovanceTcpNet`
- `InovanceConnectedCipNet`
- `InovanceEasyNet`
- `FatekProgramOverTcp`
- `FujiSPBOverTcp`
- `FujiSPHNet`
- `FujiCommandSettingType`
- `GeSRTPNet`
- `LSFastEnet`
- `LSCnetOverTcp`
- `KeyenceNanoSerialOverTcp`
- `KeyenceKvOld`
- `KeyenceMcNet`
- `KeyenceMcAsciiNet`
- `MegMeetSerialOverTcp`
- `MegMeetTcpNet`
- `PanasonicMewtocolOverTcp`
- `PanasonicMcNet`
- `ToyoPuc`
- `VigorSerialOverTcp`
- `XinJESerialOverTcp`
- `XinJETcpNet`
- `XinJEInternalNet`
- `DigitronCPLOverTcp`
- `MemobusTcpNet`
- `MemobusUdpNet`
- `YokogawaLinkTcp`
- `DLT645OverTcp`
- `DLT645With1997OverTcp`
- `SiemensS7Net`
- `SiemensS7Plus`
- `SiemensFetchWriteNet`
- `SiemensPPIOverTcp`
- `SiemensWebApi`

Serial clients such as `ModbusRtu`, `ModbusAscii`, `MelsecA3CNet`, `MelsecA4CNet`,
`MelsecFxLinks`, `MelsecFxSerial`, `LSCnet`, `OmronHostLink`, and
`OmronHostLinkCMode`, `SiemensPPI`, and `AllenBradleyDF1Serial` are covered in
the serial section below. Delta `DeltaSerial` and `DeltaSerialAscii` are also
serial clients. Inovance `InovanceSerial` and `InovanceComputerLink` are also
serial clients. Fatek `FatekProgram` is also a serial client. Fuji `FujiSPB`
is also a serial client.

## Connection refused

Typical message fragments:

```text
refused
actively refused
connection refused
Socket Exception
```

Likely causes:

1. Wrong IP address.
2. Wrong port. Modbus TCP/UDP usually uses `502`; Mitsubishi QnA 3E often uses
   `6000`, but the port is not open by default and must be configured in the PLC;
   Omron CIP, Allen-Bradley CIP, Allen-Bradley Micro800, Allen-Bradley PCCC,
   Allen-Bradley SLC, and Inovance Connected CIP usually use `44818`; Siemens
   S7 and S7 Plus use `102`; Beckhoff ADS TCP usually uses `48898`; CIMON HMI
   uses `10260`; Inovance EasyNet has no universal default port and the Demo
   uses `12939`; FatekProgramOverTcp has no universal default port and the Demo
   uses `2000`; FujiSPBOverTcp has no universal default port and the Demo uses
   `9600`; Siemens Fetch/Write has no universal default port; Siemens WebApi
   uses `443` by default. `FujiSPHNet` defaults to `18245` in the constructor;
   the Demo UI default is `507`. `FujiCommandSettingType` has no universal
   default port and the Demo uses `7000`.
   `GeSRTPNet` uses `18245` by default.
   `LSFastEnet` defaults to `2004`.
   `LSCnetOverTcp` has no universal default port; the Demo uses `2000`.
   `MelsecA3CNetOverTcp` and `MelsecA4CNetOverTcp` have no universal default
   port; the Demo uses `6000`.
   `KeyenceNanoSerialOverTcp` and `KeyenceKvOld` use `8501` by default.
   Keyence MC classes have no constructor default; the Demo uses `6000`.
   `PanasonicMewtocolOverTcp` has no universal default and the Demo uses
   `2000`. `PanasonicMcNet` has no constructor default and the Demo uses
   `5000`. `ToyoPuc` has no constructor default and the Demo uses `6000`.
   `VigorSerialOverTcp` has no constructor default and the Demo uses `5000`.
   `MemobusTcpNet` and `MemobusUdpNet` default to `502`; the Demo uses `9999`.
   `YokogawaLinkTcp` has no constructor default and the Demo uses `12289`.
   `DLT645OverTcp` defaults to `502`; the Demo uses `2000`.
   `DLT645With1997OverTcp` defaults to `502`; the Demo uses `2000`.
3. The device does not have its TCP or UDP server enabled.
4. A firewall is blocking the port.

Suggested checks:

- Confirm the PLC/device IP and port with the device manual or vendor tool.
- Enable the device TCP or UDP server and save the configuration.
- For Mitsubishi, configure the Ethernet port with Mitsubishi programming
  software and power-cycle the PLC.
- For Siemens, enable PUT/GET in the PLC hardware configuration and download it
  again. See
  [http://www.hsltechnology.cn:7900/Doc/HslCommunication](http://www.hsltechnology.cn:7900/Doc/HslCommunication).
- For `SiemensWebApi`, enable the Web server on the PLC module and confirm the
  tag is visible to the Web API.
- Temporarily test the same IP and port with another compatible client.
- Check the firewall rules on the PLC side and on the PC side.

## Receive timeout

Typical message fragments:

```text
Receive Data Time out
timeout
Time out
```

Likely causes:

1. Protocol mismatch. For example, the device expects binary frames but the
   client uses ASCII frames, or the device expects RTU-over-TCP but the client
   uses standard Modbus TCP.
2. Wrong station or network/PLC parameter. Modbus clients use `Station` or
   `s=<station>;address`; Melsec MC clients use `NetworkNumber`, `PLCNumber`,
   `NetworkStationNumber`, and `TargetIOStation`; A1E clients use `PLCNumber`;
   A3C, A4C, FxLinks, and LSCnet clients use `Station`.
3. Wrong address area, function code, or address.
4. Device configuration changed but has not been applied, and the device needs
   a restart.

Suggested checks:

- Confirm binary vs ASCII frame and transport protocol.
- Try a minimal known address before reading complex values.
- Restart the device after changing protocol or server settings.

## Station not match

Typical message fragments:

```text
Station not match
```

The exact `Station not match` client error is returned by the RTU response
parser, so it applies to:

- `ModbusRtuOverTcp`
- `ModbusRtu`

`ModbusTcpNet` and `ModbusUdpNet` use `StationCheckMatch` while matching
responses; a mismatched station there is more likely to appear as a timeout
instead of this exact message.

Likely causes:

- The station in the address does not match the device station.
- The `Station` property is wrong.

Suggested checks:

- Set `device.Station` to the device station.
- Or write the station directly in the address, for example `s=2;100`.

For Melsec MC clients, station is not a generic property. Configure
`NetworkNumber`, `PLCNumber`, `NetworkStationNumber`, and `TargetIOStation`
instead. A1E clients use `PLCNumber`. A3C, A4C, FxLinks, and LSCnet clients do
expose `Station`.

## Serial client troubleshooting

This section applies to the supported serial clients:

- `ModbusRtu`, `ModbusAscii`
- `MelsecA3CNet`, `MelsecA4CNet`, `MelsecFxLinks`, `MelsecFxSerial`
- `LSCnet`
- `OmronHostLink`, `OmronHostLinkCMode`
- `InovanceComputerLink`
- `FatekProgram`
- `FujiSPB`
- `KeyenceNanoSerial`
- `MegMeetSerial`
- `PanasonicMewtocol`
- `VigorSerial`
- `XinJESerial`
- `DigitronCPL`
- `DLT645`
- `DLT645With1997`

When serial data is missing, incomplete, or incorrect, check in this order:

1. Serial driver and port:
   - Confirm the COM port appears in Device Manager.
   - Reinstall or update the USB-to-serial converter driver if the port is
     missing or has an error.
2. Serial parameters:
   - Verify port name, baud rate, data bits, stop bits, and parity against the
     device manual.
   - For Modbus RTU, parity and baud rate must match the device exactly.
   - For Modbus ASCII, confirm the device uses ASCII mode, not RTU mode.
3. Serial cable and converter:
   - Check TX/RX or RS485 A/B wiring.
   - Try another USB-to-serial or RS485 converter.
   - Check grounding and cable length.
   - Add proper termination if the bus is long or noisy.
4. Protocol-level checks:
   - Confirm the device station number and address.
   - For `ModbusRtu`, verify CRC checks; `Crc16CheckEnable` controls CRC checking.
   - For `ModbusAscii`, verify the device expects ASCII framing.
5. Isolation test:
   - Use a serial debugging tool to send a known request and observe the raw
     response before switching to HslCommunication.

## General socket error

If the message contains `Socket Exception`, note the exception text and check:

- IP address is reachable with `ping`.
- Port is open.
- The device is powered on and connected.

## PLC disconnects periodically or randomly after a stable connection

If a PLC connection works normally, then drops briefly and recovers, classify
the behavior:

### Periodic disconnection

If disconnection occurs at a regular interval, suspect the PC network adapter
driver or another driver conflict.

Known example:

- A user's PLC disconnected about every 20 minutes.
- The root cause was a conflict between the PC camera driver and the network
  adapter driver.
- Uninstalling the camera driver resolved the issue.

Suggested checks:

- Record whether the disconnection interval is stable.
- Update the network adapter driver.
- Check whether other recently installed drivers may conflict with the network
  adapter.

### Random disconnection

If disconnection is irregular, suspect IP address conflict or contention on the
same address.

Suggested check:

1. Unplug the network cable from the PLC.
2. Try to `ping` the PLC IP address from the PC.
3. If the address still responds, another device is using the same IP address.

If the IP still responds after the PLC is physically disconnected, resolve the
duplicate IP address first.

## PLC closes idle connections automatically

Some PLCs intentionally close a connection after a period of no communication.
This is normal behavior for certain protocols.

A common example is Omron CIP. The PLC may close the connected CIP session after
roughly one minute without read/write traffic.

Suggested handling:

- Keep the connection alive by reading a known address every few seconds.
- Treat a subsequent reconnect failure as a separate issue; the initial idle
  disconnection itself is not necessarily a fault.
- Use online monitoring logic to detect and recover the connection.

## Write result is success but the PLC value did not change

If `Write(...)` returns `IsSuccess == true`, but the electrical engineer
reports that the PLC value did not change, and the issue is intermittent or
difficult to reproduce, prioritize address ownership and address overlap.

Recommended first step:

- Ask the PLC engineer to allocate a brand-new address that is guaranteed not
  to be used anywhere else, then test writes to that address.
- This quickly determines whether the original address is being overwritten by
  another part of the PLC program or by another device.

Common root causes reported by users:

- The PLC program accidentally overwrites the same address elsewhere.
- The address is mapped to another module.
- An HMI or touch screen also writes to the same address.
- Another client or communication path writes to the same register.

HslCommunication already checks both the response message structure and the
PLC write status code. If the write status were incorrect, the library would
normally return a failed `OperateResult`. Therefore, a successful write result
can generally be treated as a successful communication write. When the value
still appears unchanged, focus on address ownership, program logic, HMI writes,
and other devices sharing that address.

## User asks which PLC models a protocol supports

If the user asks which PLC models are supported by a protocol, and neither the
protocol reference nor the HslCommunication documentation clearly states the
model list, do not guess.

Search the internet for the protocol, PLC family, and vendor documentation, then
return the best available compatibility information to the user.
