#!/usr/bin/env python3
import sys
import os
import re
import importlib
import inspect
import enum

def PrintUsage():
    print("""
USAGE:  pydown.py infile.py outfile.md
""")
    return 1

def LoadModule(inPythonFileName):
    dir = os.path.dirname(inPythonFileName)
    if not dir:
        dir = os.getcwd()
    sys.path.append(dir)
    modname = os.path.basename(inPythonFileName)
    if modname.endswith('.py'):
        modname = modname[:-3]  # chop off '.py'
    module = importlib.import_module(modname)
    return module

def HtmlEscape(text):
    text = text.replace('&', '&amp;')
    text = text.replace('<', '&lt;')
    text = text.replace('>', '&gt;')
    return text

def SymbolLink(name):
    if 'a' <= name[0] <= 'z':
        # Assume built-in Python identifier, so do not link
        return '`{0}`'.format(name)
    # [`astro_time_t`](#astro_time_t)
    return '[`{0}`](#{0})'.format(name)

class ParmInfo:
    def __init__(self, name, type):
        self.name = name
        self.type = type
        self.description = ''

    def AppendDescriptionLine(self, line):
        self.description += line.strip() + ' '

class DocInfo:
    def __init__(self, doc):
        self.description = ''
        self.parameters = []
        self.attributes = []
        self.enumValues = []

        lines = doc.split('\n')

        # First line is boldfaced if followed by blank line.
        if len(lines) >= 2 and lines[0].strip() != '' and lines[1].strip() == '':
            self.summary = lines[0]
            lines = lines[2:]
        else:
            self.summary = ''

        currentAttr = None
        currentParm = None
        mode = ''    
        for line in lines:
            if re.match(r'^\-+$', line):
                continue
            if line in ['Parameters', 'Returns', 'Example', 'Examples', 'Attributes', 'Values']:
                mode = line
                continue
            if line.strip() == '':
                mode = ''
                continue
            if mode == 'Parameters':
                currentParm = self.ProcessParmAttrLine(line, currentParm, self.parameters)
            elif mode == 'Attributes':
                currentAttr = self.ProcessParmAttrLine(line, currentAttr, self.attributes)
            elif mode == 'Returns':
                pass
            elif mode == 'Example' or mode == 'Examples':
                pass
            elif mode == 'Values':
                self.ProcessEnumValue(line)
            elif mode == '':
                self.description += line + '\n'
            else:
                raise Exception('Unknown mode = "{}"'.format(mode))

    def ProcessEnumValue(self, line):
        m = re.match(r'^\s*([A-Za-z][A-Za-z0-9_]+)\s*:\s*(.*)$', line)
        if not m:
            raise Exception('Invalid enum documentation: "{}"'.format(line))
        pair = (m.group(1), m.group(2).strip())
        self.enumValues.append(pair)

    def ProcessParmAttrLine(self, line, item, itemlist):
        if line.startswith(' '):
            # The first line of description, or another line of description.
            item.AppendDescriptionLine(line)
        else:
            # name : type
            token = line.split(':')
            if len(token) != 2:
                raise Exception('Expected name:type but found: "{}"'.format(line))
            item = ParmInfo(token[0].strip(), token[1].strip())
            itemlist.append(item)
        return item

    def Table(self, itemlist, tag):
        md = ''
        if itemlist:
            md += '| Type | {} | Description |\n'.format(tag)
            md += '| --- | --- | --- |\n'
            for p in itemlist:
                md += '| {} | {} | {} |\n'.format(SymbolLink(p.type), '`' + p.name + '`', p.description.strip())
            md += '\n'
        return md

    def EnumTable(self):
        md = ''
        if self.enumValues:
            md += '| Value | Description |\n'
            md += '| --- | --- |\n'
            for (name, desc) in self.enumValues:
                md += '| {} | {} |\n'.format('`' + name + '`', desc)
        return md

    def Markdown(self):
        md = '\n'
        if self.summary:
            md += '**' + self.summary + '**\n\n'
        if self.description:
            md += self.description + '\n\n'
        md += self.Table(self.parameters, 'Parameter')
        md += self.Table(self.attributes, 'Attribute')
        md += self.EnumTable()
        md += '\n'
        return md

    def VerifyEnum(self, members):
        defs = set(name for (name, _) in self.enumValues)
        if defs != members:
            print('Actual enums: [' + ', '.join(members) + ']')
            print('Documented enums: [' + ', '.join(defs) + ']')
            raise Exception('Documented enums do not match actual enums.')

def MdSignature(sig):
    text = str(sig)
    text = HtmlEscape(text)
    return text

def MdFunction(func):
    md = ''
    doc = inspect.getdoc(func)
    if doc:
        sig = inspect.signature(func)
        md += '\n'
        md += '---\n'
        md += '\n'
        md += '<a name="{}"></a>\n'.format(func.__name__)
        md += '### ' + func.__name__ + MdSignature(sig) + '\n'
        info = DocInfo(doc)
        md += info.Markdown()
        md += '\n'
    return md

def MdClass(c):
    md = ''
    doc = inspect.getdoc(c)
    if doc:
        md += '\n'
        md += '---\n'
        md += '\n'
        md += '<a name="{}"></a>\n'.format(c.__name__)
        md += '### class ' + c.__name__ + '\n'
        info = DocInfo(doc)
        md += info.Markdown()
        md += '\n'
    return md

def MdEnumType(c):
    md = ''
    doc = inspect.getdoc(c)
    if doc:
        md += '\n'
        md += '---\n'
        md += '\n'
        md += '<a name="{}"></a>\n'.format(c.__name__)
        md += '### ' + c.__name__ + '\n'
        info = DocInfo(doc)
        info.VerifyEnum(set(c.__members__))
        md += info.Markdown()
        md += '\n'
    return md

def Markdown(module):
    md = ''
    funclist = []
    classlist = []
    enumlist = []
    errlist = []
    for name, obj in inspect.getmembers(module):
        if not name.startswith('_'):
            if inspect.isfunction(obj):
                funclist.append(obj)
            elif inspect.isclass(obj):
                if issubclass(obj, enum.Enum):
                    enumlist.append(obj)
                elif issubclass(obj, Exception):
                    errlist.append(obj)
                else:
                    classlist.append(obj)
            elif inspect.ismodule(obj):
                pass # ignore other modules pulled in
            else:
                print('pydown.py WARNING: ignoring', name)

    md += '---\n'
    md += '\n'
    md += '<a name="classes"></a>\n'
    md += '## Classes\n'
    md += '\n'
    for c in classlist:
        md += MdClass(c)

    md += '---\n'
    md += '\n'
    md += '<a name="enumerations"></a>\n'
    md += '## Enumerated Types\n'
    md += '\n'
    for c in enumlist:
        md += MdEnumType(c)

    if False:   # not yet ready to generate Markdown for error types
        md += '---\n'
        md += '\n'
        md += '<a name="errors"></a>\n'
        md += '## Error Types\n'
        md += '\n'
        for c in errlist:
            md += MdErrType(c)

    md += '---\n'
    md += '\n'
    md += '<a name="functions"></a>\n'
    md += '## Functions\n'
    md += '\n'
    for func in funclist:
        md += MdFunction(func)
    
    return md

def main():
    if len(sys.argv) != 3:
        return PrintUsage()
    inPythonFileName = sys.argv[1]
    outMarkdownFileName = sys.argv[2]
    # Delete output file before we begin.
    # That way, if anything goes wrong, it won't exist,
    # and thus the error becomes conspicuous to scripts/tools.
    if os.access(outMarkdownFileName, os.F_OK):
        os.remove(outMarkdownFileName)
    module = LoadModule(inPythonFileName)
    md = Markdown(module)
    with open(outMarkdownFileName, 'wt') as outfile:
        outfile.write(md)
    return 0

if __name__ == '__main__':
    sys.exit(main())