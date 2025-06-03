This is a simple custom reverse proxy written in YARP. It proxies requests to two backend services, and collects metrics
using Prometheus and Grafana. There are actually two implementations of the reverse proxy. The Gateway project is something
I vibe-coded using Gemini 2.5 Pro in Cursor. It works but is somewhat messy. The ManualGateway project I coded myself by 
reading the YARP documentation and looking at the code samples in YARP's GitHub repository. It feels much cleaner and written
in a more advanced way, where one modifies the YARP pipeline to achieve dynamic and programmatic routing. Read more about 
the implementation and the conclusions of my little vibe-coding vs. classical coding experiment in [my blog](https://aljazkovac.github.io/posts/yarp/).