//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace gestionaMelon
{
    using System;
    using System.Collections.Generic;
    
    public partial class AspNetRoles
    {
        public AspNetRoles()
        {
            this.AspNetUsers = new HashSet<AspNetUsers>();
        }
    
        public string Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<AspNetUsers> AspNetUsers { get; set; }
    }
}
