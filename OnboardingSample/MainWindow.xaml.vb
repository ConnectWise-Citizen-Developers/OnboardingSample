Imports System.Windows.Controls.Primitives
Imports ConnectWiseDotNetSDK.ConnectWise.Client
Imports ConnectWiseDotNetSDK.ConnectWise.Client.Common.Model
Imports ConnectWiseDotNetSDK.ConnectWise.Client.Company.Api
Imports ConnectWiseDotNetSDK.ConnectWise.Client.Company.Model
Imports ConnectWiseDotNetSDK.ConnectWise.Client.Service.Api
Imports ConnectWiseDotNetSDK.ConnectWise.Client.Service.Model
Imports ConnectWiseDotNetSDK.ConnectWise.Client.System.Api
Imports ConnectWiseDotNetSDK.ConnectWise.Client.System.Model


Class MainWindow
    Public SelectedCompanyIDasString As String
    Public SelectedContactAsString As String
    Public TicketParameters = "", cert = "", vendor = "", integration = "", resold = "", product = ""
    Public workflow = 0, newTicketStatus = 0, newTicketSTatusEmail = 0, ticketRespondedStatus = 0, inventBoard = 0, inventBoardStatus = 0, inventNewTicketStatus = 0, inventRespondedTicketStatus = 0, inventCompanyStatus = 0, LoginCompany = "", LoginSite = "", PublicKey = "", PrivateKey = "" 'Live/iDev


    Public Function getApiClient()
        Return New ApiClient("API Samples", LoginSite, LoginCompany).SetPublicPrivateKey(PublicKey, PrivateKey)
    End Function
    Public Function SearchCompanies()
        LB_Outputs.Items.Clear()
        Dim client = getApiClient()
        Dim companiesapi = New CompaniesApi(client)
        Dim response = companiesapi.GetCompanies("name like ""*" + TB_Company.Text.ToString + "*""")
        For Each company In response.GetResult(Of Company())
            LB_Outputs.Items.Add("ID: " + company.Id.ToString + " Status: " + company.Status.Name.ToString + " Name: " + company.Name.ToString)
        Next
        If response.IsSuccessResponse Then
            AuditBox.Items.Add("Found Companies")
        Else
            AuditBox.Items.Add("Search Failed SearchCompanies")
        End If
    End Function

    Private Sub BTN_Search_Click(sender As Object, e As RoutedEventArgs) Handles BTN_Search.Click
        SearchCompanies()
    End Sub

    Private Sub BTN_Process_Click(sender As Object, e As RoutedEventArgs) Handles BTN_Process.Click
        If LB_Outputs.SelectedItem = Nothing Then
            AuditBox.Items.Add("Please pick your company")
        Else
            SelectedCompanyIDasString = LB_Outputs.SelectedItem.ToString
            PR_ProcessText()

            For Each selecteditem In LB_Parameters.SelectedItems
                If selecteditem.content = "Set Company Status" Then
                    AuditBox.Items.Add("Running Set Company Status")
                    PR_SetCompanyStatus()
                ElseIf selecteditem.content = "Create Invent Ticket" Then
                    AuditBox.Items.Add("Running Create Invent Ticket")
                    TicketParameters = ""
                    cert = ""
                    vendor = ""
                    integration = ""
                    resold = ""
                    product = ""
                    PR_FindCompanyandContactForTicket()
                ElseIf selecteditem.content = "Add to Workflow Rules" Then
                    AuditBox.Items.Add("Running Add to Workflow Rules")
                    PR_UpdateWorkflowRules()
                End If
            Next
        End If
    End Sub


    Private Sub btn_certified_Click(sender As Object, e As RoutedEventArgs) Handles btn_certified.Click
        grid_certification.Visibility = Visibility.Hidden
        grid_product.Visibility = Visibility.Visible
        cert = "Certified"
    End Sub
    Private Sub btn_power_Click(sender As Object, e As RoutedEventArgs) Handles btn_power.Click
        grid_certification.Visibility = Visibility.Hidden
        grid_product.Visibility = Visibility.Visible
        cert = "Power"
    End Sub
    Private Sub btn_master_Click(sender As Object, e As RoutedEventArgs) Handles btn_master.Click
        grid_certification.Visibility = Visibility.Hidden
        grid_product.Visibility = Visibility.Visible
        cert = "Master"
    End Sub
    Private Sub btn_manage_Click(sender As Object, e As RoutedEventArgs) Handles btn_Manage.Click
        grid_product.Visibility = Visibility.Hidden
        grid_vendor.Visibility = Visibility.Visible
        product = "CW"
    End Sub
    Private Sub btn_both_Click(sender As Object, e As RoutedEventArgs) Handles btn_Both.Click
        grid_product.Visibility = Visibility.Hidden
        grid_vendor.Visibility = Visibility.Visible
        product = "CW/LT"
    End Sub
    Private Sub btn_automate_Click(sender As Object, e As RoutedEventArgs) Handles btn_Automate.Click
        grid_product.Visibility = Visibility.Hidden
        grid_vendor.Visibility = Visibility.Visible
        product = "LT"
    End Sub
    Private Sub btn_existingvendor_Click(sender As Object, e As RoutedEventArgs) Handles btn_existingvendor.Click
        grid_vendor.Visibility = Visibility.Hidden
        grid_int.Visibility = Visibility.Visible
        vendor = "E"
    End Sub

    Private Sub btn_newvendor_Click(sender As Object, e As RoutedEventArgs) Handles btn_newvendor.Click
        grid_vendor.Visibility = Visibility.Hidden
        grid_int.Visibility = Visibility.Visible
        vendor = "N"
    End Sub
    Private Sub btn_existingInt_Click(sender As Object, e As RoutedEventArgs) Handles btn_existingInt.Click
        grid_int.Visibility = Visibility.Hidden
        grid_resold.Visibility = Visibility.Visible
        integration = "A"
    End Sub

    Private Sub btn_newInt_Click(sender As Object, e As RoutedEventArgs) Handles btn_newInt.Click
        grid_int.Visibility = Visibility.Hidden
        grid_resold.Visibility = Visibility.Visible
        integration = "B"
    End Sub
    Private Sub btn_resold_Click(sender As Object, e As RoutedEventArgs) Handles btn_resold.Click
        grid_resold.Visibility = Visibility.Hidden
        resold = "R"
        PR_CreateTicket()
    End Sub
    Private Sub btn_notresold_Click(sender As Object, e As RoutedEventArgs) Handles btn_notresold.Click
        grid_resold.Visibility = Visibility.Hidden
        resold = ""
        PR_CreateTicket()
    End Sub






    Public Function PR_ProcessText()
        SelectedCompanyIDasString = SelectedCompanyIDasString.Remove(0, 4)
        SelectedCompanyIDasString = SelectedCompanyIDasString.Substring(0, SelectedCompanyIDasString.IndexOf(" ")).Trim
    End Function

    Private Sub AuditBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles AuditBox.SelectionChanged
        If AuditBox.SelectedItem.ToString Like "Created Ticket*" Then
            Dim ticketid As String = AuditBox.SelectedItem.ToString.Remove(0, 18)

            Process.Start("https://" + LoginSite + "/v4_6_release/services/system_io/Service/fv_sr100_request.rails?service_recid=" + ticketid + "&companyName=" + LoginCompany)
        End If
    End Sub

    Public Function PR_SetCompanyStatus()
        Dim client = getApiClient()
        Dim companiesapi = New CompaniesApi(client)
        Dim patch As New List(Of PatchOperation)
        Dim patchitem = New PatchOperation With {.Op = "replace", .Path = "status/id", .Value = inventCompanyStatus}
        patch.Add(patchitem)

        Dim Response = companiesapi.UpdateCompanyById(SelectedCompanyIDasString, patch)
        If Response.IsSuccessResponse Then
            AuditBox.Items.Add("Patched Company Status for ID: " + SelectedCompanyIDasString)
        Else
            AuditBox.Items.Add("Patch Failed PR_SetCompanyStatus")
        End If
    End Function
    Public Function PR_UpdateWorkflowRules()


        Dim client = getApiClient()
        Dim checkworkfloweventsapi = New WorkflowEventsApi(client)
        Dim check = checkworkfloweventsapi.GetEventsCount(workflow, "eventcondition contains """ + SelectedCompanyIDasString.ToString + """")
        If check.GetResult(Of Count).count > 0 Then
            AuditBox.Items.Add("Workflow Exists")
        Else
            WF_RunWorkflow(workflow, ticketRespondedStatus)
            WF_RunWorkflow(workflow, newTicketStatus, newTicketSTatusEmail)
        End If

    End Function
    Public Function WF_RunWorkflow(ByVal workflow, ByVal firststatus, Optional secondstatus = 0)
        Dim client = getApiClient()
        Dim workfloweventsapi = New WorkflowEventsApi(client)
        Dim eventconditionstring As String

        If secondstatus <= 0 Then
            eventconditionstring = "({""id"":33,""triggerId"":33,""value"":{""value"":" + firststatus.ToString + "}}) and {""id"":35,""triggerId"":95,""value"":{""value"":" + SelectedCompanyIDasString + "}}"
        Else
            eventconditionstring = "({""id"":33,""triggerId"":33,""value"":{""value"":" + firststatus.ToString + "}} or {""id"":34,""triggerId"":33,""value"":{""value"":" + secondstatus.ToString + "}}) and {""id"":35,""triggerId"":95,""value"":{""value"":" + SelectedCompanyIDasString + "}}"
        End If

        Dim workflowevent As New WorkflowEvent With {.EventCondition = eventconditionstring.ToString, .FrequencyUnit = 0, .FrequencyOfExecution = 5, .ExecutionTime = 2}
        Dim eventresponse = workfloweventsapi.CreateEvent(workflow, workflowevent)

        If eventresponse.IsSuccessResponse Then
            AuditBox.Items.Add("Created Workflow Event")
            Dim workflowactionapi = New WorkflowActionsApi(client)

            Dim notifytype As New NotifyTypeReference With {.Id = 3}
            Dim status As New ServiceStatusReference

            If secondstatus <= 0 Then
                status.Id = inventRespondedTicketStatus
            Else
                status.Id = inventNewTicketStatus
            End If

            Dim workflowaction As New WorkflowAction With {.NotifyType = notifytype, .BoardStatus = status}

            Dim actionresponse = workflowactionapi.CreateAction(workflow, eventresponse.GetResult(Of WorkflowEvent).Id, workflowaction)
            If actionresponse.IsSuccessResponse Then
                AuditBox.Items.Add("Created Workflow Action ID: " + actionresponse.GetResult(Of WorkflowAction).Id.ToString)
            Else
                AuditBox.Items.Add("Failed to add WF Action, must manually add the change status part")
            End If
        Else
            AuditBox.Items.Add("Failed to create workflow event")
        End If
    End Function

    Public Function PR_CreateTicket()
        Dim client = getApiClient()
        Dim ticketsapi = New TicketsApi(client)
        Dim company As New CompanyReference
        Dim contact As New ContactReference
        company.Id = SelectedCompanyIDasString
        contact.Id = SelectedContactAsString
        Dim todaysdate As Date = Date.Now()
        Dim formatteddate As String = todaysdate.ToString("MMddyy")

        If resold = Nothing Then
            TicketParameters = " (" + cert + ") " + product + " (" + vendor + "/" + integration + ") Invent"
        Else
            TicketParameters = " (" + cert + ") " + product + " (" + vendor + "/" + integration + "/" + resold + ") Invent"
        End If



        Dim Ticket As New Ticket With {.Summary = "Invent Program: " + formatteddate + TicketParameters, .Company = company, .Contact = contact, .Board = New BoardReference With {.Id = inventBoard}, .Status = New ServiceStatusReference With {.Id = inventBoardStatus}}
        Dim response = ticketsapi.CreateTicket(Ticket)

        If response.IsSuccessResponse Then
            AuditBox.Items.Add("Created Ticket #: " + response.GetResult(Of Ticket).Id.ToString)
        Else
            AuditBox.Items.Add("Ticket Creation Failure PR_CreateTicket")
        End If
    End Function
    Public Function PR_FindCompanyandContactForTicket()
        Dim client = getApiClient()
        Dim contactapi = New ContactsApi(client)
        Dim company As New CompanyReference
        company.Id = SelectedCompanyIDasString
        Dim conditions = "company/id = " + company.Id.ToString

        If contactapi.GetContactsCount(conditions).GetResult(Of Count).count > 0 Then
            Dim Response = contactapi.GetContacts(conditions,,,,, 1000)
            If Response.IsSuccessResponse Then
                AuditBox.Items.Add("Found Contacts")
            Else
                AuditBox.Items.Add("Coundn't Find Contacts PR_FindCompanyandContactForTicket")
            End If
            LB_Contacts.Items.Clear()

            For Each contact In Response.GetResult(Of Contact())
                LB_Contacts.Items.Add("ID: " + contact.Id.ToString + " " + contact.FirstName + " " + contact.LastName)
            Next
            LB_Contacts.Visibility = Visibility.Visible
            BTN_Select_Contact.Visibility = Visibility.Visible
            AuditBox.Items.Add("Displaying Contact Selection")
        Else
            AuditBox.Items.Add("No Contacts for Company")
        End If
    End Function
    Private Sub BTN_Select_Contact_Click(sender As Object, e As RoutedEventArgs) Handles BTN_Select_Contact.Click
        LB_Contacts.Visibility = Visibility.Hidden
        BTN_Select_Contact.Visibility = Visibility.Hidden

        SelectedContactAsString = LB_Contacts.SelectedItem.ToString
        SelectedContactAsString = SelectedContactAsString.Remove(0, 4)
        SelectedContactAsString = SelectedContactAsString.Substring(0, SelectedContactAsString.IndexOf(" ")).Trim
        grid_certification.Visibility = Visibility.Visible

    End Sub


End Class
