# OpenP2P
Peer-to-Peer networking library for thousands of players simulatenously.

# Design

###### Networking Core Design

Client or Server  ->  Data Controller

NetworkProtocol ->  Simplify message management
  
NetworkMessage  ->  Custom Message Protocols
  
NetworkStream   ->  ByteBuffer Write/Read
  
NetworkSocket   ->  Interfaces with low-level sockets


###### Multi-threaded

1 send thread

X listener threads, where X is total clients and/or servers running together.

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
