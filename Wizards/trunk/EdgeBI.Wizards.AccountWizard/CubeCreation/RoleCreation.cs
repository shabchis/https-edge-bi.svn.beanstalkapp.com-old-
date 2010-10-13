using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AnalysisServices;


namespace EdgeBI.Wizards.AccountWizard.CubeCreation
{
    public class RoleCreation
    {
        public bool RoleUpdate(Database db, string roleName, string roleId, string roleMember)
        {
            Role newRole = new Role();

            newRole = CreateRole(roleName, db, roleId, roleMember);

            if (newRole != null)
            {
                try
                {
                    newRole.Update();
                }
                catch (Exception ex)
                {
                   // MessageBox.Show("Role Update error: " + ex.ToString());
                }
                //_database.Update();
                return true;
            }
            else
                return false;
        }
        private Role CreateRole(string roleName, Database db, string roleId, string roleMember)
        {
            Role role = null;
            try
            {
                role = db.Roles.Add(db.Roles.GetNewName(roleId));
                role.Name = roleName;
            }
            catch
            {
                //Role already exists with the same name
               // MessageBox.Show("Role: \"" + roleName + "\" already exists in database.");
                return null;
            }
            try
            {
                AssignMember(role, roleMember);
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.ToString());
            }
            return role;
        }

        private void AssignMember(Role role, string memberName)
        {
            if (role != null)
                role.Members.Add(new RoleMember(memberName));
            //return role;
        }
    }
}
