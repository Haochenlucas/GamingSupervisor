from sklearn.externals import joblib
import sys
# hidden reference for pyinstaller
from sklearn.linear_model import Perceptron


# load pickled object
s = joblib.load("model.pkl")

# run until ends
while True:
    # reads line without new lines (\r\n)
    aa = raw_input().rstrip()
    # parsed line
    X = [[float(x) for x in aa.split(" ")]]
    # predicts and prints to stdout
    print s.predict(X)[0]
    sys.stdout.flush()
