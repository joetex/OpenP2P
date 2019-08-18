# OpenP2P
Peer-to-Peer networking library for thousands of players simulatenously.

# Design

###### Networking Core Design

```
NetworkClient/NetworkServer 

  -> NetworkProtocol 
  
    -> NetworkSocket
    
      -> NetworkThread
      
    -> NetworkChannel
    
      -> NetworkPacket
      
        ->NetworkMessage
```

###### Message System

NetworkChannel defines the types of messages that can be created.  NetworkMessage derived classes perform the serialization into the packet buffer.  You create a message, configure the message variables, then send a ReliableMessage, Message, or Response.  ReliableMessage will expect a Response type message from their target endpoint.  Message and Response are unreliable messages.

In future NetworkPeer and NetworkPeerGroup will allow stacking messages into a single packet, when possible.  

NetworkMessage header encodes 5 bytes for ChannelType (up to 16 channels, 4 bits), SendType (Message/Response 1 bit), Reliable (yes/no 1 bit), Redirect (add 2 bytes for user id), Message Owner ID (2 bytes), Message Sequence (2 bytes).

###### Event System

Current event system allows hooking into the NetworkChannelEvents OnMessage and OnResponse for when a NetworkMessage is received.  There are also low level NetworkSocket events OnSend and OnReceive, for further customization. 

###### Multi-threaded

Send, Receive, and Receive Processing are the 3 sub threads on top of main thread.

###### Goals

1) Socket Connection between Client/Server (DONE)

2) Byte stream writer and reader (DONE)

3) Protocol defining method of request/respond and route messages (DONE)

4) Defining customized NetworkMessages (DONE)

5) Create Reliable UDP Messages

6) Build basic server to connect peers together

7) Field test p2p connections

8) Build QuadTree/Octree/Grid of peers on server

9) Build algorithms to group peers into sub-networks

10) Build local simulation for simulating p2p network

11) Build simple games in Unity using network

12) Keep it going
