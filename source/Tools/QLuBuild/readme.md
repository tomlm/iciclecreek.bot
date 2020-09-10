![icon](icon.png)

# QLuBuild
This command line tool processes .qna files and generates .dialog files which use QLucene as a recognizer

## Installation
To install:

```dotnet tool install --global qlubuild```

This has dependency on bf-cli
```npm install -g bf-cli ```

## Usage
```
QLuBuild folder [--prebuild]
--prebuild - prebuild cached catalog in {qnaFile}.catalog folder.
```
if --prebuild is specified it will create a catalog in a subfolder instead of creating a qna.json file. This catalog is preindexed so it should be faster to load and use less memory.

## Output
Each {filename}.{language}.qna file will have the following generated:

```
foo.en-us.qna
foo.en-us.qna.json 
foo.en-us.qna.catalog 
foo.en-us.qna.dialog
foo.dialog
```

Where
* **{lang}.qna** file is the original markdown qna for that language
* **{lang}.qna.json** file is the qna in json form for that language 

                      _(if --prebuild is not specified, the json file is indexed in RAMDirectory on the fly)_
* **{lang}.qna.catalog** folder containing the qna lucene index for that language 

                      _(if --prebuild is specified)_
* **.{lang}.qna.dialog** is the QLuceneRecognizer bound to that the {lang}.qna.json file
* **.qna.dialog** is the a MultiLanguageRecognizer bound to each language recognizer for the qna.

