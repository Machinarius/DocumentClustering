Attributions
============

WEKA Machine Project
--------------------
Stopword list taken from the WEKA project, originally located at https://svn.cms.waikato.ac.nz/svn/weka/trunk/weka/src/main/java/weka/core/Stopwords.java

This word list is used to filter unimportant words from the english language to enhance the output of the algorithms.

Porter stemming algorithm
-------------------------
Taken almost verbatim from https://tartarus.org/martin/PorterStemmer/csharp.txt

This algorithm further refines the word-to-term discard process by matching words to their stems, for example:

````
CONNECT
CONNECTED
CONNECTING
CONNECTION
CONNECTIONS
````

All of these words can be assumed to have roughly the same meaning in any context as their stem: CONNECT.


By mapping all of these words to the term CONNECT, the implementation trades off some contextual accuracy to increase the speed and overall document matching ability of the system.
More information about this can be found on https://tartarus.org/martin/PorterStemmer/def.txt
