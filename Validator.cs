using System;
using System.Data;
using System.Net.Mail;

public class Validator
{
    private DataTable rolesTable;
    private DataTable accountsTable;
    private DataTable clientsTable;
    private DataTable employeesTable;
    private DataTable policiesTable;
    private DataTable policyTypeTable;
    private DataTable policyAndTypeTable;
    private DataTable clientAgentsTable;
    private DataTable claimStatusHistory;
    private DataTable payoutsTable;

    public Validator(DataTable rolesTable = null, DataTable accountsTable = null, DataTable clientsTable = null, DataTable employeesTable = null, DataTable policiesTable = null, DataTable policyTypeTable = null, DataTable policyAndTypeTable = null, DataTable clientAgentsTable = null, DataTable claimStatusHistory = null, DataTable payoutsTable = null)
    {
        this.rolesTable = rolesTable;
        this.accountsTable = accountsTable;
        this.clientsTable = clientsTable;
        this.employeesTable = employeesTable;
        this.policiesTable = policiesTable;
        this.policyTypeTable = policyTypeTable;
        this.policyAndTypeTable = policyAndTypeTable;
        this.clientAgentsTable = clientAgentsTable;
        this.claimStatusHistory = claimStatusHistory;
        this.payoutsTable = payoutsTable;
    }

    public bool ValidateRoleName(string roleName)
    {
        if (string.IsNullOrEmpty(roleName) || roleName.Length > 50)
        {
            return false;
        }

        foreach (DataRow row in rolesTable.Rows)
        {
            if (row["role_name"].ToString() == roleName)
            {
                return false;
            }
        }

        return true;
    }

    public bool ValidateUsername(string username, int? accountId = null)
    {
        if (string.IsNullOrEmpty(username) || username.Length > 50)
        {
            return false;
        }

        foreach (DataRow row in accountsTable.Rows)
        {
            if (accountId != null && (int)row["account_id"] == accountId && row["username"].ToString() == username)
            {
                continue;
            }

            if (row["username"].ToString() == username)
            {
                return false;
            }
        }

        return true;
    }

    public bool ValidatePassword(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Length <= 25;
    }

    public bool ValidateName(string name)
    {
        return !string.IsNullOrEmpty(name) && name.Length <= 100;
    }

    public bool ValidateDate(DateTime? date)
    {
        return date != null && date != default(DateTime);
    }

    public bool ValidatePhone(string phone, int? Id = null)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length > 15)
        {
            return false;
        }

        foreach (char c in phone)
        {
            if (!char.IsDigit(c))
            {
                return false;
            }
        }

        if (clientsTable != null)
        {
            foreach (DataRow row in clientsTable.Rows)
            {
                if (Id != null && (int)row["client_id"] == Id && row["phone"].ToString() == phone)
                {
                    continue;
                }
                if (row["phone"].ToString() == phone)
                {
                    return false;
                }
            }
        }

        if (employeesTable != null)
        {
            foreach (DataRow row in employeesTable.Rows)
            {
                if (Id != null && (int)row["employee_id"] == Id && row["phone"].ToString() == phone)
                {
                    continue;
                }
                if (row["phone"].ToString() == phone)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool ValidateEmail(string email, int? Id = null)
    {
        try
        {
            MailAddress mailAddress = new MailAddress(email);
            if (email.Length > 100)
            {
                return false;
            }

            if (clientsTable != null)
            {
                foreach (DataRow row in clientsTable.Rows)
                {
                    if (Id != null && (int)row["client_id"] == Id && row["email"].ToString() == email)
                    {
                        continue;
                    }

                    if (row["email"].ToString() == email)
                    {
                        return false;
                    }
                }
            }

            if (employeesTable != null)
            {
                foreach (DataRow row in employeesTable.Rows)
                {
                    if (Id != null && (int)row["employee_id"] == Id && row["email"].ToString() == email)
                    {
                        continue;
                    }

                    if (row["email"].ToString() == email)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateAddress(string address)
    {
        return address.Length <= int.MaxValue;
    }

    public bool ValidateRoleId(object roleId)
    {
        return roleId != null && int.TryParse(roleId.ToString(), out int id);
    }

    public bool ValidateAccountId(object accountId, int? Id = null)
    {
        if (accountId != null && int.TryParse(accountId.ToString(), out int id))
        {
            if (clientsTable != null)
            {
                foreach (DataRow row in clientsTable.Rows)
                {
                    if (Id != null && (int)row["client_id"] == Id && (int)row["account_id"] == id)
                    {
                        continue;
                    }
                    if ((int)row["account_id"] == id)
                    {
                        return false;
                    }
                }
            }

            if (employeesTable != null)
            {
                foreach (DataRow row in employeesTable.Rows)
                {
                    if (Id != null && (int)row["employee_id"] == Id && (int)row["account_id"] == id)
                    {
                        continue;
                    }
                    if ((int)row["account_id"] == id)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        return false;
    }

    public bool ValidateClientId(object clientId, int? Id = null)
    {
        if (clientId != null && int.TryParse(clientId.ToString(), out int id))
        {
            if (policiesTable != null)
            {
                foreach (DataRow row in policiesTable.Rows)
                {
                    if (Id != null && (int)row["policy_id"] == Id && (int)row["client_id"] == id)
                    {
                        continue;
                    }
                    if ((int)row["client_id"] == id)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        return false;
    }

    public bool ValidatePremiumAmount(decimal premiumAmount)
    {
        return premiumAmount > 0 && premiumAmount.ToString().Length <= 10;
    }

    public bool ValidateCoverageAmount(decimal coverageAmount)
    {
        return coverageAmount > 0 && coverageAmount.ToString().Length <= 10;
    }

    public bool ValidateTypeName(string typeName, int? typeId = null)
    {
        if (string.IsNullOrEmpty(typeName) || typeName.Length > 50)
        {
            return false;
        }

        foreach (DataRow row in policyTypeTable.Rows)
        {
            if (typeId != null && (int)row["type_id"] == typeId && row["type_name"].ToString() == typeName)
            {
                continue;
            }

            if (row["type_name"].ToString() == typeName)
            {
                return false;
            }
        }

        return true;
    }

    public bool ValidateDescription(string descriprion)
    {
        return !string.IsNullOrEmpty(descriprion);
    }

    public bool ValidatePolicyTypeMapping(int? policyId, int? typeId)
    {
        // Проверка, что policyId и typeId не являются нулевыми
        if (policyId == null || typeId == null)
        {
            return false;
        }

        // Проверка уникальности сочетания policyId и typeId
        foreach (DataRow row in policyAndTypeTable.Rows)
        {
            if ((int)row["policy_id"] == policyId && (int)row["type_id"] == typeId)
            {
                return false; // Дублирующая запись найдена
            }
        }

        return true; // Сочетание уникально
    }

    public bool ValidateClientAgentMapping(int? clientId, int? employeeId)
    {
        // Проверка, что clientId и employeeId не являются нулевыми
        if (clientId == null || employeeId == null)
        {
            return false;
        }

        // Проверка уникальности сочетания clientId и employeeId
        foreach (DataRow row in clientAgentsTable.Rows)
        {
            if ((int)row["client_id"] == clientId && (int)row["employee_id"] == employeeId)
            {
                return false; // Дублирующая запись найдена
            }
        }

        return true; // Сочетание уникально
    }

    public bool ValidateAmount(decimal amount)
    {
        return amount > 0 && amount.ToString().Length <= 10;
    }

    public bool ValidateComboBoxSelection(int? selectedValue)
    {
        // Проверка, что selectedValue не является нулевым
        return selectedValue != null;
    }

    public bool ValidateComboBoxSelection(string selectedValue)
    {
        return !string.IsNullOrEmpty(selectedValue);
    }

    public bool ValidatePayoutUniqueness(int? claimId, int? currentPayoutId = null)
    {
        if (claimId == null)
        {
            return false;
        }

        foreach (DataRow row in payoutsTable.Rows)
        {
            if ((int)row["claim_id"] == claimId && (currentPayoutId == null || (int)row["payout_id"] != currentPayoutId))
            {
                return false; // Дублирующая запись найдена
            }
        }

        return true; // Претензия уникальна
    }
}
