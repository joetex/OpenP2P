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

NetworkMessage header encodes 5 bytes for ChannelType (up to 16 channels, 4 bits), SendType (Message/Response 1 bit), Reliable (yes/no 1 bit), Redirect (1 bit +  2 bytes for user id), Message Owner ID (2 bytes), Message Sequence (2 bytes).

###### Event System

Current event system allows hooking into the NetworkChannelEvents OnMessage and OnResponse for when a NetworkMessage is received.  There are also low level NetworkSocket events OnSend and OnReceive, for further customization. 

###### Multi-threaded

Send, Receive, and Receive Processing are the 3 sub threads on top of main thread.

###### Goals

Build P2P system that can send reliable and unreliable messages, and connect users
