robocopy Columnizer deploy/Example /E /XA:HS /XD .svn /XD bin /XD obj /XF *.user 
robocopy ContextMenu deploy/Example /E /XA:HS /XD .svn /XD bin /XD obj /XF *.user 
robocopy ProcessLauncher deploy/Example /E /XA:HS /XD .svn /XD bin /XD obj /XF *.user 
robocopy CsvColumnizer deploy/Example/CsvColumnizer /E /XA:HS /XD .svn /XD bin /XD obj /XF *.user 
robocopy Log4jXmlColumnizer deploy/Example/Log4jXmlColumnizer /E /XA:HS /XD .svn /XD bin /XD obj /XF *.user 
copy API\LogExpert-API.chm deploy
copy "HelpSmith\SDK.chm" deploy

pause
