# Quick Troubleshooting Checklist

## Before Starting
- [ ] Both devices on same Wi-Fi network (or one is hotspot)
- [ ] Both devices have .NET 10 runtime installed
- [ ] Application built successfully

## Step 1: Launch Application
- [ ] Run app on both devices
- [ ] Check **Debug → Output Window** for startup messages
- [ ] Verify you see:
  ```
  === Application Starting ===
  Local IP: [your IP]
  Machine Name: [your computer name]
  === Network Services Started ===
  ```

## Step 2: Check Firewall
- [ ] Open Windows Defender Firewall with Advanced Security
- [ ] Go to **Inbound Rules → New Rule**
- [ ] Add rule for:
  - Protocol: UDP, Port: 8888 → Action: Allow
  - Protocol: TCP, Port: 8889 → Action: Allow
- [ ] Repeat for **Outbound Rules**

## Step 3: Test Discovery
- [ ] Click **Refresh** button on Device A
- [ ] Check Output window for messages like:
  ```
  Broadcasting to: 192.168.1.255
  Peer Discovered: DeviceName at 192.168.1.X
  ```
- [ ] Device B should appear in "Discovered Players" list in ~5 seconds

## Step 4: If Still Not Working

### Check Network Configuration
```powershell
ipconfig
# Device A: 192.168.1.100
# Device B: 192.168.1.X (same first three octets = same network)
```

### Check if Ports Are Blocked
```powershell
# Test UDP 8888
netstat -ano | findstr 8888

# Test TCP 8889
netstat -ano | findstr 8889
```

### Check Output Window for Specific Errors
Common errors and solutions:
- `Broadcasting to:` missing → Firewall might be blocking
- `Peer Discovered:` works but not connecting → TCP port 8889 blocked
- Connection timeout → Peer not running or firewall blocking

### Temporary Firewall Test (Windows)
```powershell
# DISABLE FIREWALL (testing only!)
netsh advfirewall set allprofiles state off

# Run application and test discovery

# RE-ENABLE FIREWALL
netsh advfirewall set allprofiles state on
```

## Step 5: Try Direct Connection

If peer discovery works but connections fail:

**Edit MainViewModel.cs temporarily:**
```csharp
// In InvitePlayer method, replace:
// bool connected = await _network.ConnectToPeerAsync(ipAddress);

// With hardcoded IP for testing:
bool connected = await _network.ConnectToPeerAsync("192.168.1.101"); // Use actual IP
```

If direct IP connection works → Issue is with broadcast
If it fails → TCP port 8889 or network connectivity issue

## Required Changes Made

✅ **NetworkDiscovery.cs**
- Uses actual subnet broadcast addresses instead of `IPAddress.Broadcast`
- Broadcasts on all active network interfaces
- Added comprehensive error logging

✅ **NetworkManager.cs**
- Added 5-second connection timeout
- Improved TCP listener to accept multiple connections
- Added detailed error logging for all operations

✅ **MainViewModel.cs**
- Added debug output for startup and peer discovery
- Better error reporting

## Ports Reference

| Protocol | Port | Purpose |
|----------|------|---------|
| UDP | 8888 | Peer discovery (broadcast) |
| TCP | 8889 | Game communication |

Both must be allowed in firewall for full functionality.

## Still Stuck?

1. **Check Output Window** - contains all error messages
2. **Look for these keywords** in Output:
   - `Broadcasting to:` → Discovery attempted
   - `Peer Discovered:` → Found another device
   - `Connected to peer` → TCP connection successful
   - `Error:` → Specific problem identified

3. **Share the Output window messages** - they'll show exactly what's happening

## One More Thing

After any firewall changes:
1. Close the application completely
2. Wait 10 seconds
3. Restart the application
4. Click Refresh button

Firewall rules take a moment to apply.
