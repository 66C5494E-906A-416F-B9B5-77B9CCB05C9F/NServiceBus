﻿. .\TestSupport.ps1

Cleanup

..\.\bin\debug\Runner.exe PubSub numberofsubscribers=10 numberofmessages=10000 numberofthreads=10 storage=msmq	

Cleanup


..\.\bin\debug\Runner.exe PubSub numberofsubscribers=10 numberofmessages=10000 numberofthreads=10 storage=inmemory

Cleanup

..\.\bin\debug\Runner.exe PubSub numberofsubscribers=10 numberofmessages=10000 numberofthreads=10 storage=raven

Cleanup
