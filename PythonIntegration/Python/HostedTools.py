'''
Defines the base class as well as some utility functions for creating HostedTools tools in Python
'''
import json
import traceback
import sys
from contextlib import contextmanager

_ObjectCollection = []

_globalMonitor = None
_globalTool = None

def globalWriteRow(obj):
    if (_globalMonitor and _globalTool):
        _globalTool.WriteRow(_globalMonitor,obj)

def globalWriteLine(msg):
    if (_globalMonitor):
        _globalMonitor.Writer.WriteLine(msg)
        
class _MonitorAsFile :
    def __init__(self,monitor):
        self._oldOut = sys.stdout
        self._monitor = monitor
    # close() is an error
    def flush():
        pass
    def fileno():
        return self._oldOut.fileno()
    # isatty (don't implement)
    # read, seek, truncate methods not implemented; error
    def write(self,msg):
        self._monitor.Writer.Write(msg)
    def writelines(self,seq):
        for s in seq:
            _monitor.Writer.WriteLine(str(s))
    def __getattr__(self,attr):
        return getattr(self._oldOut,attr)
        
@contextmanager
def redirectToMonitor(monitor):
    ''' Run code within this context manager to redirect standard out to the supplied monitor object '''
    new_target = _MonitorAsFile(monitor)
    old_target, old_err, sys.stderr, sys.stdout = sys.stdout, sys.stderr, new_target, new_target
    try:
        yield new_target
    finally:
        sys.stdout = old_target
        sys.stderr = old_err

def _mainEval(cmd) :
	'''Overcome problem in Python.Runtime.PythonEngine.RunString'''
	exec cmd in globals()
         
class HostedToolBase(object) :
	'''Base class for Python classes that define Hosted Tools'''
	def __init__(self,name,menuItems=None,settingDefinitions=None,settingNames=None,isGrid=False,customToolName=None) :
		'''Sets properties of the hosted tool

		name -- Name of the tool, must be unique across all tools; used as an action target
		menuItems -- List of menuItems created by the tool; often will include one with the tool name as the action.  The menuItems should be the actual CLR MenuItem object.
		settingDefinitions -- Setting definitions defined by this tool, if any.  Setting definitions should be the actual CLR objection.
		settingNames -- List of setting id's that the tool will edit, in the order they should appear within the tool
		isGrid -- Set to true if the the tool requires grid output
		customToolName -- Plugin name of plugin class to associate with this Python object instead of the default PythonTool class
		'''

		self._name = name
		self._settingDefinitions = settingDefinitions or []
		self._menuItems = menuItems or []
		self._settingNames = settingNames or []
		self._isGrid = isGrid
		self._customToolName = customToolName
		_ObjectCollection.append(self)
	def SetHostedTool(self,d) :
		'''Called from CLR side to specify the CLR object providing services to the tool'''
		self._hostedTool = d
	def Value(self,i) :
		'''Return a setting value from the SettingManager'''
		return self._hostedTool.Value(i)
	def SetValue(self,i,v) :
		'''Set a value in the SettingManager'''
		self._hostedTool.SetValue(i,v)
	def WriteRow(self,monitor,row) :
		'''Write a python object to the output grid as if it were an IHtValue instance'''
		self._hostedTool.WriteRow(monitor,json.dumps(row))
	def FromHtValue(self,row) :
		'''Convert an IHtValue object to the corresponding Python object'''
		return json.loads(self._hostedTool.SerializeHtValue(row))
	def Perform(self,monitor) :
          '''Catch exceptions from sub-class, and write to monitor'''
          global _globalTool, _globalMonitor
          try:
              _globalTool = self
              _globalMonitor = monitor
              self.PerformPython(monitor)
              _globalTool = None
              _globalMonitor = None
          except:
              monitor.Writer.WriteLine("Python exception:")
              monitor.Writer.WriteLine(traceback.format_exc())
              _globalTool = None
              _globalMonitor = None
              raise

