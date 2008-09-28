
##############################################################################
# Written by:  Cachen Chen <cachen@novell.com>
# Date:        07/22/2008
# Description: button.py wrapper script
#              Used by the button-*.py tests
##############################################################################$

import sys
import os
import actions
import states

from strongwind import *
from button import *


# class to represent the main window.
class ButtonFrame(accessibles.Frame):

    # constants
    # the available widgets on the window
    BUTTON_ONE = "button1"
    BUTTON_TWO = "button2"
    BUTTON_TREE = "button3"
    BUTTON_FOUR = "button4"


    def __init__(self, accessible):
        super(ButtonFrame, self).__init__(accessible)
        self.button1 = self.findPushButton(self.BUTTON_ONE)
        self.button2 = self.findPushButton(self.BUTTON_TWO)
        self.button3 = self.findPushButton(self.BUTTON_TREE)
        self.button4 = self.findPushButton(self.BUTTON_FOUR)
 
    #give 'click' action
    def click(self,button):
        button.click()

    #check the Label text after click button2
    def assertLabel(self, labelText):
        procedurelogger.expectedResult('Label text has been changed to "%s"' % labelText)
        self.findLabel(labelText)

    #rise message frame window after click button1
    def assertMessage(self):
        self.message = self.app.findFrame('message')

        self.message.findPushButton('OK').click()

    #assert button can implement Image provider
    def assertImage(self, button, imageSize=False):
        procedurelogger.action("assert %s's image" % button)
        image = button.__getattr__("imageSize")

        procedurelogger.expectedResult('"%s" image size is %s' % (button, image))
        if imageSize == True:
            assert str(image) != '(-1, -1)', "image NotImplementedError or incorrect size"
        elif imageSize:
            assert str(image) == '(-1, -1)', "image NotImplementedError or incorrect size"
    
    #close application main window after running test
    def quit(self):
        self.altF4()
