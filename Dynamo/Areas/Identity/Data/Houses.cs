using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dynamo.Data;


public class Houses
{
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int id {  get; set; }

}
