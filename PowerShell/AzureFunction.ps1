# how to deploye Teams cmdlets in Azure Functions
# http://www.modernworkplacesolutions.rocks/microsoft-teams-automation-teams-powershell-in-azure-functions/

$secpasswd = ConvertTo-SecureString '%YOUR_PWD%' -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("user@tenant.onmicrosoft.com", $secpasswd)

Write-Output "got this command: " $REQ_QUERY_cmd

switch($REQ_QUERY_cmd){
                        "GetTeam"
                                {
                                    #$body = "GetTeam"
                                    Connect-MicrosoftTeams -Credential $mycreds
                                    $teams = Get-Team -User "user@tenant.onmicrosoft.com"
                                    $body = $teams | ConvertTo-Json
                                }
                        "NewTeam"
                                {
                                    #$body = Get-Content $req
                                    Write-Output Get-Content $req
                                    $content = Get-Content $req -Raw | ConvertFrom-Json
                                    Write-Output "trying to create team with Displayname: " + $content.displayName + "and alias: " + $content.alias 
                                    Connect-MicrosoftTeams -Credential $mycreds
                                    $newTeam = New-Team -DisplayName $content.displayName -Alias $content.alias
                                    $body = $newTeam.GroupId
                                     
                                }
                        default 
                                {   
                                    $body = "CMD param contains no known command"
                                }

}

Write-Output $body

$message = "{ `"headers`":{`"content-type`":`"text/plain`"}, `"body`":`"$body`"}"
[System.IO.File]::WriteAllText($res,$message)

