namespace TP6_Gorojod_Schwartz_Waserman;

public class Partida {
    public int Id {get;set;}
    public DateTime fechaInicio {get;set;}
    public DateTime horaInicio {get;set;}
    public int IdSala {get;set;}
    public string estadoActual {get;set;}
    public string nombreJugador {get;set;}

    public Partida(){
        
    }
}