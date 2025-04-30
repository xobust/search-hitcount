
# MetaSearch 

![image](https://github.com/user-attachments/assets/b20b88e1-91fe-4573-9229-0492060c74ab)


This is a simple search engine that shows the ammount of search hits for different search engines. 
It was done for a technical interview at Voyado.



## Functional requirements
- Your website should contain an input field where a user can enter one or more
words.
- Your service should then make a search against two or more search engines (e.g.,
Google, Bing, Yahoo, Twitter, Web Search, Algolia, AltaVista…) and present the
total number of search hits from each search engine. Just to be clear, it is only the
number of hits that should be presented to the user, not the hits themselves.
- If the user enters several words, your service should make a search for each word,
summarize the number of hits, and then present the sum from each search engine.
Example: If the user enters “Hello world” your service should (for every search
provider) first make the search “Hello” which might return together 54M hits and
then search for “world” which returns 100M hits. Then 154M should be presented to
the user.

## Supported engines

I used this site as reference for what serch engine builds their own indexes:
https://seirdy.one/posts/2021/03/10/search-engines-with-own-indexes/


> [!NOTE]
> I could not get Bing to work as Microsoft has shut down Bing v7 api for some reason. The scraper also did not include hit count.

- [Google (custom search engine)](https://developers.google.com/custom-search/v1/overview)
- [Mojeek](https://www.mojeek.com/support/api/search) 
- [Searchapi](https://searchapi.io) 
 they have a few different scraped search providers

## Configuration 
The following secrets needs to be set
```
SearchApiIoEngine:API_KEY
MojeekSearchEngine:API_KEY
GoogleCustomSearchEngine:SEARCH_ENGINE_ID
GoogleCustomSearchEngine:API_KEY
```
