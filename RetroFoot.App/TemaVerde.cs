using Avalonia;
using Avalonia.Media;

namespace RetroFoot.App;

// As cores do visual Moderno (Configuracoes.Tema). Tres paletas com os mesmos nomes:
//  - Verde: o verde e amarelo da marca e do hub (o quadro verde da capa, os cartoes
//    verde-escuros com o fio dourado, os icones amarelos, o Jogar em pilula ouro);
//  - Escuro: o clean escuro, "refletor": grafite esverdeado, texto giz (nao branco puro), um
//    acento so (o ambar apagado das lampadas do estadio), tudo liso (sem degrades nem fio
//    dourado: os cartoes se separam pelo tom), sombras leves. Para sessoes longas;
//  - Claro: o clean claro: cinza-claro frio (nao branco puro), texto grafite, o acento no verde
//    do gramado. Para jogar de dia e no celular ao sol.
// E a UNICA paleta do Moderno: as paletas de cada tela (ClubeModerno, MM, ConfigEstilo, os M* da
// TelaEquipa, da TelaJornada...) apontam para estes nomes (como propriedades, para a troca de
// tema chegar nelas), e um ajuste aqui muda todas. O Classico nao usa nada daqui.
//
// Como usar:
//  - Fundo / FundoTela: o fundo da tela inteira (FundoTela e o degrade do quadro, para telas
//    cheias; Fundo e a cor lisa, para onde precisa de cor solida); FundoEscuro e o mais fundo
//    (barras de rolagem, por tras das molduras).
//  - Barra: as barras de cima (menus, titulo); Cartao: os cartoes (mais escuros que o fundo,
//    como os botoes do hub), com Borda (o fio dourado-oliva); CartaoForte: o que fica dentro
//    de um cartao e se destaca (pilulas, campos, botoes secundarios); Menu: os menus abertos.
//  - Linha: separadores e trilhos de barras; Zebra: as linhas pares das tabelas; Hover: o rato
//    em cima; Selecao: a linha escolhida (com um traco Destaque a esquerda); Humano: a linha da
//    equipa do jogador nas tabelas.
//  - Texto, TextoSuave (as descricoes do hub), TextoFraco (cabecalhos, numeros secundarios),
//    Apagado (desabilitado). Nao assuma texto branco: no Claro o Texto e grafite.
//  - Destaque: o acento (acoes, abas escolhidas, foco, icones); SobreDestaque: o texto sobre
//    o acento; Ouro: o acento mais fundo (bordas, hover); BotaoPrimario: a pilula do Jogar.
//  - Estados (texto/icone sobre o fundo): Verde (positivo, ao vivo), Vermelho, Laranja, Azul,
//    Roxo, Prata. Preenchimentos fortes (bolinhas, selos): VerdeForte, CinzaForte, VermelhoForte.
//  - Sombras: SombraCartao, SombraBotao, SombraDialogo (mais leves nos clean).
// As cores tambem existem como Color em TemaVerde.Cores (para degrades e misturas).
public enum TemaCor { Verde, Escuro, Claro }

public static class TemaVerde
{
    // RETROFOOT_TEMA=verde|escuro|claro passa por cima das Configuracoes (testes, demos)
    public static TemaCor Atual { get; private set; } = System.Environment.GetEnvironmentVariable("RETROFOOT_TEMA") switch
    {
        "verde" => TemaCor.Verde, "escuro" => TemaCor.Escuro, "claro" => TemaCor.Claro, _ => Configuracoes.Ler().Tema,
    };
    public static bool Claro => Atual == TemaCor.Claro;
    // os clean (Escuro e Claro): lisos, sem o quadro em degrade nem o fio dourado
    public static bool Clean => Atual != TemaCor.Verde;
    // o tema do Fluent por baixo (caixas de texto, interruptores, rolagem): claro so no Claro
    public static Avalonia.Styling.ThemeVariant Variante => Claro ? Avalonia.Styling.ThemeVariant.Light : Avalonia.Styling.ThemeVariant.Dark;

    // (preguicosa: os Tons abaixo ainda nao existem quando Atual e lido)
    static Paleta? _p;
    static Paleta P => _p ??= Paleta.De(Atual);

    // troca a paleta; quem chama avisa as telas (Visual.Aplicar)
    public static void Aplicar(TemaCor t)
    {
        if (t == Atual) return;
        Atual = t;
        _p = Paleta.De(t);
    }

    // as janelas sobre a capa da marca (o Sobre da abertura) ficam no Verde em qualquer tema:
    // using (TemaVerde.Marca()) { monta a janela } (as cores sao lidas ao montar)
    public static System.IDisposable Marca()
    {
        var (atual, p) = (Atual, _p);
        Atual = TemaCor.Verde;
        _p = Paleta.De(TemaCor.Verde);
        return new Volta(() => { Atual = atual; _p = p; });
    }

    sealed class Volta(System.Action a) : System.IDisposable { public void Dispose() => a(); }

    // os tons de uma paleta
    sealed class Tons
    {
        public Color Fundo, FundoClaro, FundoBorda, FundoEscuro, Barra, Cartao, CartaoForte, Menu, Borda, Linha, Zebra, Hover, Selecao, Humano;
        public Color Texto, TextoSuave, TextoFraco, Apagado, Destaque, DestaqueClaro, Ouro, SobreDestaque;
        public Color Verde, Vermelho, Laranja, Azul, Roxo, Prata, VerdeForte, CinzaForte, VermelhoForte, DiscoLogo;
        public Color CirculoIcone, Dialogo, Sombra, Campeao;
        public double AlfaVidro, AlfaDestaqueFundo, AlfaVeu;
        public string SombraCartao = "", SombraBotao = "", SombraDialogo = "";
    }

    static Color C(string s) => Color.Parse(s);

    static readonly Tons TonsVerde = new()
    {
        Fundo = C("#0D5A2E"), FundoClaro = C("#11683A"), FundoBorda = C("#0A4A25"), FundoEscuro = C("#073A1D"),
        Barra = C("#073A1D"), Cartao = C("#094622"), CartaoForte = C("#0F5A31"), Menu = C("#0A4D27"),
        Borda = C("#4E7A2E"), Linha = C("#1A6A3A"), Zebra = C("#0B4E27"), Hover = C("#13663C"), Selecao = C("#1A7442"), Humano = C("#3B6B22"),
        Texto = C("#FFFFFF"), TextoSuave = C("#C8E6D2"), TextoFraco = C("#9DC8AB"), Apagado = C("#5E8C6C"),
        Destaque = C("#F8D734"), DestaqueClaro = C("#FFE46A"), Ouro = C("#DDA917"), SobreDestaque = C("#0B4A25"),
        Verde = C("#6EE89A"), Vermelho = C("#FF8A80"), Laranja = C("#FFB566"), Azul = C("#8EC8FF"), Roxo = C("#CBA8FF"), Prata = C("#D3E2D8"),
        VerdeForte = C("#22A355"), CinzaForte = C("#5F806B"), VermelhoForte = C("#E5483B"), DiscoLogo = C("#E6F0E9"),
        CirculoIcone = C("#062A15"), Dialogo = C("#0A4523"), Sombra = C("#000000"), Campeao = C("#F8D734"),
        AlfaVidro = 0.80, AlfaDestaqueFundo = 0.18, AlfaVeu = 0.60,
        SombraCartao = "0 4 12 0 #40000000", SombraBotao = "0 2 8 0 #50000000", SombraDialogo = "0 12 36 0 #A0000000",
    };

    // o clean escuro: os tons do fundo sobem em degraus pequenos (fundo < cartao < campo), o
    // texto e giz (#E3E8E4, nao o branco) e o ambar e mais apagado que o amarelo da marca
    static readonly Tons TonsEscuro = new()
    {
        Fundo = C("#1A211E"), FundoClaro = C("#1A211E"), FundoBorda = C("#1A211E"), FundoEscuro = C("#141A17"),
        Barra = C("#161C19"), Cartao = C("#212A26"), CartaoForte = C("#2A3530"), Menu = C("#26302B"),
        Borda = C("#323D37"), Linha = C("#2C3732"), Zebra = C("#1E2622"), Hover = C("#2B3631"), Selecao = C("#33403A"), Humano = C("#3D3C2A"),
        Texto = C("#E3E8E4"), TextoSuave = C("#C2CBC5"), TextoFraco = C("#9AA59F"), Apagado = C("#5E6A64"),
        Destaque = C("#E2B04A"), DestaqueClaro = C("#ECC36C"), Ouro = C("#C99634"), SobreDestaque = C("#1A211E"),
        Verde = C("#7CCB98"), Vermelho = C("#E8897F"), Laranja = C("#E5A766"), Azul = C("#8DB8E0"), Roxo = C("#B9A3DE"), Prata = C("#C9D1CC"),
        VerdeForte = C("#3C9A62"), CinzaForte = C("#56625C"), VermelhoForte = C("#C9554A"), DiscoLogo = C("#E3E8E4"),
        CirculoIcone = C("#141A17"), Dialogo = C("#232D28"), Sombra = C("#000000"), Campeao = C("#E2B04A"),
        AlfaVidro = 1, AlfaDestaqueFundo = 0.16, AlfaVeu = 0.55,
        SombraCartao = "0 1 2 0 #30000000", SombraBotao = "0 1 2 0 #30000000", SombraDialogo = "0 10 30 0 #70000000",
    };

    // o clean claro: o fundo e cinza-esverdeado frio (nao branco), os cartoes um degrau mais
    // claros, o texto grafite (#1E2622, nao o preto) e o acento no verde do gramado
    static readonly Tons TonsClaro = new()
    {
        Fundo = C("#E7ECE8"), FundoClaro = C("#E7ECE8"), FundoBorda = C("#E7ECE8"), FundoEscuro = C("#DAE1DC"),
        Barra = C("#F1F4F2"), Cartao = C("#F6F8F6"), CartaoForte = C("#E9EEEA"), Menu = C("#F6F8F6"),
        Borda = C("#D0D8D3"), Linha = C("#DCE2DE"), Zebra = C("#EEF2EF"), Hover = C("#E0E7E2"), Selecao = C("#D6E6DB"), Humano = C("#DCEBDD"),
        Texto = C("#1E2622"), TextoSuave = C("#3D4842"), TextoFraco = C("#5C6862"), Apagado = C("#A0AAA4"),
        Destaque = C("#2F7A4E"), DestaqueClaro = C("#3A8C5D"), Ouro = C("#25633F"), SobreDestaque = C("#FFFFFF"),
        Verde = C("#1E7D45"), Vermelho = C("#B3402F"), Laranja = C("#A35A10"), Azul = C("#2B64A0"), Roxo = C("#6A47A5"), Prata = C("#5C6862"),
        VerdeForte = C("#2E9A5A"), CinzaForte = C("#8C9892"), VermelhoForte = C("#CF4F3E"), DiscoLogo = C("#FFFFFF"),
        CirculoIcone = C("#DAE5DD"), Dialogo = C("#F6F8F6"), Sombra = C("#1E2622"), Campeao = C("#B8870E"),
        AlfaVidro = 1, AlfaDestaqueFundo = 0.14, AlfaVeu = 0.40,
        SombraCartao = "0 1 2 0 #141E2622", SombraBotao = "0 1 2 0 #1F1E2622", SombraDialogo = "0 10 30 0 #331E2622",
    };

    // os pinceis de uma paleta (feitos uma vez por troca)
    sealed class Paleta
    {
        public readonly Tons T;
        public readonly IBrush Fundo, FundoEscuro, Barra, Cartao, CartaoForte, Menu, Borda, Linha, Zebra, Hover, Selecao, Humano;
        public readonly IBrush Texto, TextoSuave, TextoFraco, Apagado, Destaque, DestaqueClaro, Ouro, SobreDestaque;
        public readonly IBrush Verde, Vermelho, Laranja, Azul, Roxo, Prata, VerdeForte, CinzaForte, VermelhoForte, DiscoLogo;
        public readonly IBrush Trilho = Alfa(Colors.White, 0.15);
        public readonly IBrush Campeao, DestaqueFundo, Sombra, Veu, FundoTela, BotaoPrimario, BotaoPrimarioHover;
        public readonly IBrush CartaoHub, BarraHub, CirculoIcone, DialogoHub, BotaoSecundario, BotaoDesligado;
        public readonly BoxShadows SombraCartao, SombraBotao, SombraDialogo;

        static readonly Paleta[] _cache = new Paleta[3];
        public static Paleta De(TemaCor t) => _cache[(int)t] ??= new Paleta(t switch { TemaCor.Escuro => TonsEscuro, TemaCor.Claro => TonsClaro, _ => TonsVerde }, t == TemaCor.Verde);

        Paleta(Tons t, bool verde)
        {
            T = t;
            Fundo = B(t.Fundo); FundoEscuro = B(t.FundoEscuro); Barra = B(t.Barra); Cartao = B(t.Cartao); CartaoForte = B(t.CartaoForte);
            Menu = B(t.Menu); Borda = B(t.Borda); Linha = B(t.Linha); Zebra = B(t.Zebra); Hover = B(t.Hover); Selecao = B(t.Selecao); Humano = B(t.Humano);
            Texto = B(t.Texto); TextoSuave = B(t.TextoSuave); TextoFraco = B(t.TextoFraco); Apagado = B(t.Apagado);
            Destaque = B(t.Destaque); DestaqueClaro = B(t.DestaqueClaro); Ouro = B(t.Ouro); SobreDestaque = B(t.SobreDestaque);
            Verde = B(t.Verde); Vermelho = B(t.Vermelho); Laranja = B(t.Laranja); Azul = B(t.Azul); Roxo = B(t.Roxo); Prata = B(t.Prata);
            VerdeForte = B(t.VerdeForte); CinzaForte = B(t.CinzaForte); VermelhoForte = B(t.VermelhoForte); DiscoLogo = B(t.DiscoLogo);
            Campeao = B(t.Campeao); DestaqueFundo = Alfa(t.Destaque, t.AlfaDestaqueFundo);
            Sombra = Alfa(t.Sombra, verde ? 0.38 : 0.18); Veu = Alfa(t.Sombra, t.AlfaVeu);
            // o quadro da capa (so no Verde): mais claro no meio de cima, mais escuro nas bordas
            FundoTela = verde ? new RadialGradientBrush
            {
                Center = new RelativePoint(0.5, 0.3, RelativeUnit.Relative),
                GradientOrigin = new RelativePoint(0.5, 0.3, RelativeUnit.Relative),
                RadiusX = new RelativeScalar(0.85, RelativeUnit.Relative), RadiusY = new RelativeScalar(0.95, RelativeUnit.Relative),
                GradientStops = { new GradientStop(t.FundoClaro, 0), new GradientStop(t.Fundo, 0.55), new GradientStop(t.FundoBorda, 1) },
            }.ToImmutable() : Fundo;
            // a pilula do Jogar: o degrade ouro do hub no Verde; o acento liso nos clean
            BotaoPrimario = verde ? Vertical("#FCE46A", "#F8D734", "#DDA917") : Destaque;
            BotaoPrimarioHover = verde ? Vertical("#FFEC8A", "#FFE04A", "#EDB92A") : DestaqueClaro;
            // o vidro do hub (meio transparente no Verde; opaco nos clean, que nao tem o quadro)
            CartaoHub = Alfa(t.Cartao, t.AlfaVidro); BarraHub = Alfa(t.Barra, t.AlfaVidro);
            CirculoIcone = Alfa(t.CirculoIcone, verde ? 0.60 : 1); DialogoHub = Alfa(t.Dialogo, verde ? 0.97 : 1);
            BotaoSecundario = verde ? Alfa(t.Barra, 0.55) : CartaoForte;
            BotaoDesligado = verde ? Alfa(t.Barra, 0.70) : Alfa(t.CartaoForte, 0.6);
            SombraCartao = BoxShadows.Parse(t.SombraCartao); SombraBotao = BoxShadows.Parse(t.SombraBotao); SombraDialogo = BoxShadows.Parse(t.SombraDialogo);
        }
    }

    public static class Cores
    {
        // a marca (a mesma nos tres temas: o hub e o logotipo)
        public static readonly Color MarcaVerdeEscuro = Color.Parse("#0D5A2E"), MarcaVerde = Color.Parse("#168C40"),
            MarcaAmarelo = Color.Parse("#F8D734"), MarcaOuro = Color.Parse("#DDA917");

        public static Color Fundo => P.T.Fundo; public static Color FundoClaro => P.T.FundoClaro; public static Color FundoEscuro => P.T.FundoEscuro;
        public static Color Barra => P.T.Barra; public static Color Cartao => P.T.Cartao; public static Color CartaoForte => P.T.CartaoForte;
        public static Color Menu => P.T.Menu; public static Color Borda => P.T.Borda; public static Color Linha => P.T.Linha;
        public static Color Zebra => P.T.Zebra; public static Color Hover => P.T.Hover; public static Color Selecao => P.T.Selecao;
        public static Color Humano => P.T.Humano;
        public static Color Texto => P.T.Texto; public static Color TextoSuave => P.T.TextoSuave; public static Color TextoFraco => P.T.TextoFraco;
        public static Color Apagado => P.T.Apagado;
        public static Color Destaque => P.T.Destaque; public static Color DestaqueClaro => P.T.DestaqueClaro; public static Color Ouro => P.T.Ouro;
        public static Color SobreDestaque => P.T.SobreDestaque;
        public static Color Verde => P.T.Verde; public static Color Vermelho => P.T.Vermelho; public static Color Laranja => P.T.Laranja;
        public static Color Azul => P.T.Azul; public static Color Roxo => P.T.Roxo; public static Color Prata => P.T.Prata;
        public static Color VerdeForte => P.T.VerdeForte; public static Color CinzaForte => P.T.CinzaForte; public static Color VermelhoForte => P.T.VermelhoForte;
        // o circulo claro por tras dos logos que somem no fundo (Escudos.PrecisaDisco)
        public static Color DiscoLogo => P.T.DiscoLogo;
        // a cor das sombras e do veu (preto no Verde e no Escuro; o grafite no Claro)
        public static Color Sombra => P.T.Sombra;
    }

    static IBrush B(Color c) => new ImmutableSolidColorBrush(c);

    public static IBrush Fundo => P.Fundo; public static IBrush FundoEscuro => P.FundoEscuro;
    public static IBrush Barra => P.Barra; public static IBrush Cartao => P.Cartao; public static IBrush CartaoForte => P.CartaoForte;
    public static IBrush Menu => P.Menu; public static IBrush Borda => P.Borda; public static IBrush Linha => P.Linha;
    public static IBrush Zebra => P.Zebra; public static IBrush Hover => P.Hover; public static IBrush Selecao => P.Selecao; public static IBrush Humano => P.Humano;
    public static IBrush Texto => P.Texto; public static IBrush TextoSuave => P.TextoSuave; public static IBrush TextoFraco => P.TextoFraco; public static IBrush Apagado => P.Apagado;
    public static IBrush Destaque => P.Destaque; public static IBrush DestaqueClaro => P.DestaqueClaro; public static IBrush Ouro => P.Ouro;
    public static IBrush SobreDestaque => P.SobreDestaque;
    public static IBrush Verde => P.Verde; public static IBrush Vermelho => P.Vermelho; public static IBrush Laranja => P.Laranja;
    public static IBrush Azul => P.Azul; public static IBrush Roxo => P.Roxo; public static IBrush Prata => P.Prata;
    public static IBrush VerdeForte => P.VerdeForte; public static IBrush CinzaForte => P.CinzaForte; public static IBrush VermelhoForte => P.VermelhoForte;
    public static IBrush DiscoLogo => P.DiscoLogo;
    // o trilho das barras de medida (moral, forca): o branco translucido nos escuros; a borda no Claro
    public static IBrush Trilho => Claro ? Borda : P.Trilho;
    // o campeao nas tabelas (o acento; no Claro um ouro, para nao confundir com o Sobe)
    public static IBrush Campeao => P.Campeao;
    // o fundo de um destaque suave (etiquetas, a aba escolhida)
    public static IBrush DestaqueFundo => P.DestaqueFundo;
    public static IBrush Sombra => P.Sombra; public static IBrush Veu => P.Veu;
    // o fundo das telas cheias: o quadro da capa em degrade no Verde; liso nos clean
    public static IBrush FundoTela => P.FundoTela;
    // a pilula do Jogar
    public static IBrush BotaoPrimario => P.BotaoPrimario; public static IBrush BotaoPrimarioHover => P.BotaoPrimarioHover;
    // o vidro do hub (TelaHub), para as paginas sobre o FundoTela (inicio, configuracoes, ferramentas):
    // CartaoHub e BarraHub (meio transparentes no Verde), CirculoIcone o circulo dos icones,
    // DialogoHub a caixa dos dialogos (com fio Destaque)
    public static IBrush CartaoHub => P.CartaoHub; public static IBrush BarraHub => P.BarraHub;
    public static IBrush CirculoIcone => P.CirculoIcone; public static IBrush DialogoHub => P.DialogoHub;
    // o botao secundario e o primario desligado
    public static IBrush BotaoSecundario => P.BotaoSecundario; public static IBrush BotaoDesligado => P.BotaoDesligado;
    // as sombras dos cartoes, dos botoes principais e dos dialogos
    public static BoxShadows SombraCartao => P.SombraCartao; public static BoxShadows SombraBotao => P.SombraBotao;
    public static BoxShadows SombraDialogo => P.SombraDialogo;

    static IBrush Vertical(string a, string b, string c) => new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative), EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
        GradientStops = { new GradientStop(Color.Parse(a), 0), new GradientStop(Color.Parse(b), 0.45), new GradientStop(Color.Parse(c), 1) },
    }.ToImmutable();

    // as cores de um tema sem troca-lo (as previas das Configuracoes)
    public static (Color Fundo, Color Cartao, Color Borda, Color Destaque, Color Fraco) Amostra(TemaCor t)
    {
        var p = Paleta.De(t).T;
        return (p.Fundo, p.Cartao, t == TemaCor.Verde ? p.Borda : p.Cartao, p.Destaque, p.TextoFraco);
    }

    // uma cor do tema com outra opacidade (0..1)
    public static IBrush Alfa(Color c, double a) => new ImmutableSolidColorBrush(Color.FromArgb((byte)System.Math.Round(255 * System.Math.Clamp(a, 0, 1)), c.R, c.G, c.B));
}
