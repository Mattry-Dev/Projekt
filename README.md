# Shop Simulator

Hi this is my project for school. It is a simulator of a department store.

basically it spawns customers who walk around, pick up items and then wait in queues to pay. multiple checkout counters work at the same time.

## How to run it
1. make sure you have dotnet installed
2. open terminal in this folder
3. type `dotnet run`
4. open your browser and go to the url it shows (localhost:something)

## Exmple Requests

/api/start

{
  "openCheckouts": 5,       // number of cashiers working
  "customerIntensity": 300, // how many people come per hour
  "simulationSpeed": 60     // speed multiplier (60 means 1 minute passes in 1 second)
}

/api/stop

stops the process and returns the report

/api/status

returns all casiers with queue count and the simulation time

/api/report

returns all past reports







