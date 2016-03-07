import json

_ObjectCollection = []

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
		return self._hostedTool.Value(i).str()
	def SetValue(self,i,v) :
		'''Set a value in the SettingManager'''
		self._hostedTool.SetValue(i,v)
	def WriteRow(self,monitor,row) :
		'''Write a python object to the output grid as if it were an IHtValue instance'''
		self._hostedTool.WriteRow(monitor,json.dumps(row))
	def FromHtValue(self,row) :
		'''Convert an IHtValue object to the corresponding Python object'''
		return json.loads(self._hostedTool.SerializeHtValue(row))
