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
QLuBuild folderWithQnAfiles
```

## Output
Each {filename}.{language}.qna file will have the following generated:

```
foo.en-us.qna
foo.en-us.qna.json
foo.en-us.qna.dialog
foo.dialog
```

Where
* ** {lang}.qna** file is the original markdown qna for that language
* ** {lang}.qna.json** file is the qna in json form for that language
* **.{lang}.qna.dialog is the QLuceneRecognizer bound to that the {lang}.qna.json file
* **.qna.dialog is the a MultiLanguageRecognizer bound to each language recognizer for the qna.

