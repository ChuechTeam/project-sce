# Chuech (PROJECT SCE)
A simple education platform with motivation and collaboration in mind.

**This project is currently under really early development.**
## Principles
* **Dumb simple**: Finding your ways through documents and administration 
shouldn't be a pain. Chuech is free of any unnecessary complexity to make sure
your brain doesn't explode.
* **Packed with content creation tools**: Documents? They're nice. But many other kinds
of content exist, providing more efficient ways to learn than by reading your lesson: flashcards, 
mind maps, quizzes, etc. Chuech gives you the tools to create those and interact with them within
the application.
* **Inciting to progress**: Let's be honest: studying is boring. We're going to
try to make it a little bit more fun with experience and levels. Anyone can strive
for being better and get rewarded by doing so.
* **Inciting to share and collaborate**: Students often create their own learning material; 
it would really be a shame to not let everyone make use of them. With Chuech's trustworthiness score
(clearly we should find a better name), you get rewarded when people successfully learn something
with your content — like StackOverflow basically. 

## Requirements
- .NET 6 SDK
- Docker

## Build and run instructions
1. Run `docker-compose up` in the project root directory
2. That's it, isn't docker awesome?

### Service endpoints
- Core API: `localhost:6502`
- OIDC (IS4): `localhost:6501`
- Web app: `localhost:6500`