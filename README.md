# Unity RC Car UDP Simulation

This Unity project is the 3D simulation counterpart for the Python-based Sensor Dashboard app. It features a Remote Control (RC) car that can be driven and controlled in real-time via UDP communication. The car also streams live camera footage back to the client application.

## 🚀 Features

* **Real-time Movement Control**: Drive the RC car (Forward, Backward, Left, Right) using UDP string commands.
* **Dynamic Camera Control**: Rotate the First-Person/On-board camera using mouse delta inputs, including a quick reset function.
* **Live Video Streaming**: Captures the camera view, compresses it into JPEG format, and streams it to the client via UDP to ensure low-latency monitoring.

## ⚙️ Architecture & Network Protocol

The system utilizes two separate UDP ports to handle control and video streaming asynchronously without blocking the main thread.

* **Port `5005` (Receive - RX)**: Listens for incoming string commands from the Python client.
* **Port `5006` (Send - TX)**: Streams JPEG byte arrays representing the live camera feed to the Python client.

### Accepted UDP Commands (Port 5005)
| Command String | Action |
| :--- | :--- |
| `FORWARD` | Moves the car forward based on `moveSpeed`. |
| `BACKWARD` | Moves the car backward. |
| `LEFT` | Rotates the car to the left based on `turnSpeed`. |
| `RIGHT` | Rotates the car to the right. |
| `STOP` | Halts movement immediately. |
| `CAMERA:<dx>:<dy>` | Rotates the camera pitch and yaw based on mouse delta. |
| `CAMERA_RESET` | Resets the camera orientation to its initial state. |

## 🛠️ Setup Instructions

### 1. RC Car Controller Setup
1. Create a 3D model/GameObject for your RC car in the Unity scene.
2. Attach the `RCCarUDPController.cs` script to the RC car GameObject.
3. Adjust the `Move Speed` and `Turn Speed` in the Inspector.

### 2. Camera Streamer Setup
1. Create a new `Camera` as a child object of the RC car (e.g., place it on the hood for a first-person view).
2. Attach the `CameraUDPStreamer.cs` script to this child Camera.
3. In the Inspector for `CameraUDPStreamer`:
   - Set the **IP Address** to your Python client's IP (default: `127.0.0.1`).
   - Configure **Resolution** (default: 320x240) and **JPG Quality**. *(Keep resolution low to stay within the 64KB UDP packet limit).*
4. Drag and drop this child Camera into the `Camera Transform` slot of the `RCCarUDPController` script attached to the parent car object.

## 📝 Scripts Overview

* **`RCCarUDPController.cs`**: Runs a background thread to listen for UDP packets on port 5005. It parses the strings and updates the car's `Transform` (position and rotation) in the `Update()` method safely.
* **`CameraUDPStreamer.cs`**: Uses a `RenderTexture` to capture the camera's view at the end of each frame, encodes it to JPG, and sends the byte array via `UdpClient` to port 5006.

## ⚠️ Notes
* Ensure that your firewall does not block UDP ports `5005` and `5006`.
* If you run the Python app and Unity on different machines, update the IP address in both `CameraUDPStreamer.cs` and the Python UDP sender script.
