Required packages:

- scikit-learn
- numpy
- scipy
- pyinstaller

To compile into executable:

`pyinstaller retreat_predict.py -F --exclude-module Tkinter -p C:\Python27\Lib\site-packages\scipy\extra-dll\ --hiddenimport sklearn.utils.sparsetools._graph_validation --hiddenimport sklearn.utils.sparsetools._graph_tools --hiddenimport scipy._lib.messagestream --hidden-import sklearn.neighbors.typedefs `
