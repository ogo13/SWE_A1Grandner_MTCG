# **Monster Trading Cards Game**


## **Description**
> This is a C# program for a Monster Trading Card Game. It includes a server that listens for incoming connections, creates a 
new task for each, and calls the HandleConnection method to process requests. The requests are parsed and handled by the 
RequestHandler class, which returns an HttpResponse object. The program also includes a DataHandler class that interacts with
a PostgreSQL database, allowing for the insertion, updating, retrieval, and deletion of data such as users, cards, packages,
scores, and trades. The program includes a Battle class that handles the logic of battles between players represented by 
UserData and Deck objects, using the Fight() method to run battles and various helper methods to determine winners and update 
logs.

## Lessons learnt
> Since this was my first C# project the things I learnt would be too many to recall right now. But I think I grasped the first 
concepts of designpatterns and the usefulness of unittests would be my top two takeaways from this project.

## Unit tests
> I have written unit tests for the following classes: Battle, GetActionHandler, PostActionHandler, PutActionHandler. 
I tested for some functionality of various methods of these classes. Since I did my tests last, I realized some design choices 
I made were not the best and I would have to refactor some of my code. I did not have time to do this. (more interfaces, better encapsuling)
I think this will also help me in my coding future.

## Time spent
> I have not tracked my time spent from the beginning. I would estimate my total time spent to about 40 hours.


## Unique Feature

> The unique feature of my project is a little graphic for the cards when the deck is displayed. I think it makes it easier to
> see the cards and their stats. 


## Github repo

> https://github.com/ogo13/SWE_A1Grandner_MTCG
