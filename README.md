# Best Stories API
The Best Stories API returns details of the top stories from [Hacker News](https://github.com/HackerNews/API).

## Assumptions
- A maximum of 200 stories can be returned from the Hacker News API.
- Clients are not expecting live data. Once the story ids have been retrieved, it is possible that the ranking will have changed by the time the story details are retrieved. Furthermore, a cache has been implemented to improve efficiency, at the expense of slightly stale data.
- Clients are not expecting a critical service. If a fault occurs calling the Hacker News API, no retry mechanism (currently) exists and the call will fail.

## Running the API
After cloning the project, it can be run from Visual Studio.

Once the project is running it can be tested in the browser with the url https://localhost:7020/storyGetBestStories/n where n is the number of stories requested (if omitted all available stories will be returned to a maximum of 200).

## Further enhancements
- Fault tolerance if required using an exponential backoff mechanism.
- Greater unit test coverage for the controller and service classes.
- Add remaining constants to configuration.
- Provide containerisation support.
- Provide OpenAPI documentation.