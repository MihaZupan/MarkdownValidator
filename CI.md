# Continuous Markdown Validation


## Sample Travis configuration

```yaml
services:
  - docker

script:
  - docker run --rm -t -v $(pwd):/app/md/ mihazupan/markdownvalidator:latest --threshold 5
```

## Sample AppVeyor configuration

ToDo

## Command-line usage

```text
Options:
  -?|-h|--help        Show help information
  -p|--path           Path to the root working directory if --config is not set (default: current directory)
  -c|--config         Path to the configuration file
  --threshold         Number of errors to allow before returning status code 1 (default: 0)
  --warningIsError    Treat warnings as errors (default: false)
  -co|--configOutput  Path for the updated configuration file
```