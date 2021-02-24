# S4

## About
S4 is adapter on top of AWS S3 handling caching, deduplication, folder compression.
The goal of S4 is to reduce the cost of object stored in S3 while allowing for faster access than Glacier. 

## Solution structure
|Directory | Description                                         |
|----------|-----------------------------------------------------|
|.git      | Git directory                                       |
|bin       | Potentialy releasable binaries                      |
|build     | Build Infrastructure                                |
|scripts   | Scripts (can be launching artifacts from binaries)  |
|src       | Sources                                             |
