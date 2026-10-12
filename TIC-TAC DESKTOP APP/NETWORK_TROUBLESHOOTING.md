# Network Discovery Troubleshooting Guide

## What Was Fixed

### 1. **Proper Subnet Broadcasting**
   - **Problem**: Using `IPAddress.Broadcast` doesn't reliably work on Wi-Fi networks
   - **Solution**: Now calculates actual subnet broadcast address for each network interface and broadcasts to each one
   - This ensures discovery works on:
     - Same Wi-Fi network
     - PC hotspot network
     - Multiple network adapters

### 2. **Better Error Logging**
   - **Problem**: Silent failures made debugging impossible
   - **Solution**: Added `Console.WriteLine()` statements throughout network code
   - You can now see:
     - When services start
     - Broadcasting locations
     - Connection attempts and errors
     - Message sending/receiving

### 3. **Connection Timeout**
   - **Problem**: TCP connections could hang indefinitely
   - **Solution**: Added 5-second timeout on connection attempts

### 4. **Peer Deduplication**
   - **Problem**: Same peer could appear multiple times in the list
   - **Solution**: Track discovered IPs per discovery cycle

### 5. **Multiple TCP Connections**
   - **Problem**: Only one peer connection at a time
   - **Solution**: TCP listener now loops to accept multiple connections

---

## Debugging Steps

### Step 1: View Debug Output
1. Run the application
2. Open **Debug → Windows → Output** (or press `Ctrl+Alt+O`)
3. Look for messages like:
   ```
   === Application Starting ===
   Local IP: 192.168.1.100
   Machine Name: MyPC
   === Network Services Started ===
   Broadcasting to: 192.168.1.255
   Peer Discovered: AnotherPC at 192.168.X.X
   ```

### Step 2: Check Firewall Settings

**Windows Defender Firewall:**
1. Open **Windows Defender Firewall → Advanced Settings**
2. Click **Inbound Rules**
3. Create new rules for:
   - **UDP Port 8888** (for discovery)
   - **TCP Port 8889** (for game communication)
4. Repeat for **Outbound Rules**

**If using third-party firewall:**
- Add exceptions for ports 8888 (UDP) and 8889 (TCP)

### Step 3: Verify Network Connectivity

Run these commands in PowerShell:

```powershell
# Check your local IP address
ipconfig

# Check if you can reach another device on the same network
ping <other-device-ip>

# Test UDP connectivity (on receiving device)
netstat -ano | findstr 8888

# Test TCP connectivity (on receiving device)
netstat -ano | findstr 8889
```

### Step 4: Manual Testing

**On Device A:**
1. Run the application
2. Note your IP address from debug output (e.g., 192.168.1.100)
3. Click **Refresh** button
4. Check Output window for "Broadcasting to:" messages

**On Device B:**
1. Run the application
2. Verify it's on the same network (similar IP prefix like 192.168.1.X)
3. Should see Device A in "Discovered Players" list within 5 seconds
4. If not, check the Output window for error messages

### Step 5: Network Interface Issues

If broadcasting works but peers aren't discovered:

```powershell
# List all network interfaces and their status
ipconfig /all

# Flush DNS (sometimes helps with network discovery)
ipconfig /flushdns
```

Issues to look for:
- Multiple network adapters (Wi-Fi + Ethernet)
- VPN active (usually blocks broadcasting)
- Adapter in "Limited or no connectivity" state
- IPv6 used instead of IPv4

---

## Common Issues & Solutions

### Issue: "No peers discovered" on same Wi-Fi

**Cause:** Firewall blocking UDP port 8888

**Solution:**
1. Check Windows Defender Firewall (see Step 2 above)
2. Temporarily disable firewall to test:
   ```powershell
   netsh advfirewall set allprofiles state off
   # Re-enable when done:
   netsh advfirewall set allprofiles state on
   ```

### Issue: "Connection refused" when inviting

**Cause:** Firewall blocking TCP port 8889

**Solution:**
1. Add TCP 8889 rule to firewall (see Step 2 above)
2. Verify with: `netstat -ano | findstr 8889`

### Issue: Works on same PC but not different devices

**Cause:** Devices on different subnets

**Solution:**
1. Verify both devices show same IP prefix:
   - `192.168.1.X` on same network
   - `192.168.0.X` is different network
2. Connect both to same Wi-Fi hotspot

### Issue: App sees itself but not other devices

**Cause:** Broadcast not reaching other subnets

**Solution:**
1. Check if routers/access points block inter-device communication
2. Some hotspots isolate devices - check hotspot settings
3. Try direct connection via known IP in code (for testing)

### Issue: Intermittent connections

**Cause:** Multiple network interfaces competing

**Solution:**
1. Disable unused network adapters:
   - Device Manager → Network Adapters → Right-click → Disable
2. Or specify which adapter to use (advanced configuration needed)

---

## Advanced: Manual Connection Testing

If automatic discovery doesn't work, test TCP directly:

**On Device B (receiving):**
```powershell
# Start a simple TCP listener on port 8889
$server = New-Object System.Net.Sockets.TcpListener([System.Net.IPAddress]::Any, 8889)
$server.Start()
Write-Host "Listening on port 8889..."
$client = $server.AcceptTcpClient()
Write-Host "Client connected from: $($client.Client.RemoteEndPoint)"
```

**On Device A (sending):**
```powershell
# Connect to Device B (replace with actual IP)
$socket = New-Object System.Net.Sockets.TcpClient
$socket.Connect("192.168.1.101", 8889)
Write-Host "Connected to Device B"
```

If this works, your network supports P2P - issue is in application.
If this fails, firewall/network is blocking the connection.

---

## Still Not Working?

1. **Check Output Window** for specific error messages
2. **Enable all firewall exceptions** (both inbound and outbound)
3. **Restart your application** after firewall changes
4. **Restart your router/hotspot** if just set up
5. **Try hardcoding IP** instead of broadcasting to isolate the issue

If you see messages like:
- `Broadcasting to: X.X.X.X` → Broadcasting works
- `Peer Discovered:` → Discovery works
- `TCP Listener started` → TCP listening works
- Errors in Output window → Check the specific error message

---

## Technical Details

**Discovery Process:**
1. App starts → Binds to UDP port 8888 on all interfaces
2. Broadcasts a `DiscoveryPing` message to each subnet's broadcast address
3. Other apps receive ping → Respond with `DiscoveryPong`
4. Original app receives pong → Adds peer to list

**Connection Process:**
1. User clicks "Invite to Game"
2. App attempts TCP connection to peer's IP on port 8889
3. Peer's TCP listener accepts the connection
4. Messages exchanged over TCP connection
5. Game can proceed

**Ports Used:**
- **UDP 8888**: Peer discovery (broadcast)
- **TCP 8889**: Game communication (point-to-point)

Both must be open in firewall for full functionality.
