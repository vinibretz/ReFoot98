# Gera a marca do ReFoot98 (RetroFoot.App/Assets/Marca e os icones do Android) com Pillow:
#   bola-256.png   a bola verde e amarela (o icosaedro truncado de verdade, com luz e costuras)
#   icone-256/512  o quadrado azul-petroleo (a area de trabalho do Windows 98) com a bola e o selo "98"
#   nome.png       "REFOOT" com o "98" na barra de titulo azul do Windows 98 (700x135, Logotipo.cs)
#   capa-16x9/4x3  a capa da tela inicial: o gramado com a bola no centro
# Uso: python scripts/gerar-marca.py   (precisa da Arial Black do Windows: C:\Windows\Fonts\ariblk.ttf)
import math
import os
from PIL import Image, ImageDraw, ImageFilter, ImageFont

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MARCA = os.path.join(RAIZ, "RetroFoot.App", "Assets", "Marca")
ANDROID = os.path.join(RAIZ, "RetroFoot.Android")
FONTE = r"C:\Windows\Fonts\ariblk.ttf"

PETROLEO = (0, 128, 128)        # a area de trabalho do Windows 98
PETROLEO_ESC = (0, 84, 88)
MARINHO = (0, 0, 128)           # a barra de titulo ativa
AZUL_TITULO = (16, 132, 208)
VERDE = (24, 140, 64)
VERDE_ESC = (10, 86, 40)
OURO = (240, 196, 32)
BRANCO = (255, 255, 255)
CINZA = (192, 192, 192)         # o cinza dos botoes do Windows 98
SOMBRA = (128, 128, 128)
PRETO = (16, 16, 16)


def bola_3d(tam, pent=PRETO, hexa=BRANCO, costura=(40, 40, 40), giro=(0.0, 0.0)):
    """A bola de verdade: o icosaedro truncado (12 pentagonos, 20 hexagonos) projetado na esfera,
    com um pentagono de frente, costuras e luz de cima a esquerda. giro: (x, y) em radianos."""
    phi = (1 + 5 ** 0.5) / 2
    def norm(v):
        m = math.sqrt(sum(c * c for c in v)); return tuple(c / m for c in v)
    def ciclos(v):
        a, b, c = v; return [(a, b, c), (b, c, a), (c, a, b)]
    cp = []  # os centros dos pentagonos: os vertices do icosaedro
    for s1 in (1, -1):
        for s2 in (1, -1):
            cp += ciclos((0, s1, s2 * phi))
    ch = [(x, y, z) for x in (1, -1) for y in (1, -1) for z in (1, -1)]  # os dos hexagonos: o dodecaedro
    for s1 in (1, -1):
        for s2 in (1, -1):
            ch += ciclos((0, s1 * phi, s2 / phi))
    # gira para que o pentagono (0, 1, phi) fique de frente, mais o giro pedido
    f = norm((0, 1, phi))
    ang = math.acos(f[2])
    def rot(v):
        x, y, z = v
        y, z = y * math.cos(-ang) - z * math.sin(-ang), y * math.sin(-ang) + z * math.cos(-ang)
        gx, gy = giro
        y, z = y * math.cos(gx) - z * math.sin(gx), y * math.sin(gx) + z * math.cos(gx)
        x, z = x * math.cos(gy) + z * math.sin(gy), -x * math.sin(gy) + z * math.cos(gy)
        return (x, y, z)
    # a distancia de cada face ao centro (aresta 1): pentagono 2.32744, hexagono 2.26728
    faces = [(rot(norm(c)), 2.32744, True) for c in cp] + [(rot(norm(c)), 2.26728, False) for c in ch]
    s = 2
    T = tam * s
    im = Image.new("RGBA", (T, T), (0, 0, 0, 0))
    px = im.load()
    R = T / 2 - s
    luz = norm((-0.45, -0.55, 0.7))
    for j in range(T):
        for i in range(T):
            x, y = (i + 0.5 - T / 2) / R, (j + 0.5 - T / 2) / R
            r2 = x * x + y * y
            if r2 > 1:
                continue
            p = (x, y, math.sqrt(1 - r2))
            # a face que o raio do centro atravessa primeiro: a de maior (n.p)/d
            m1 = m2 = -9.0
            e1 = False
            for n, d, e in faces:
                v = (n[0] * p[0] + n[1] * p[1] + n[2] * p[2]) / d
                if v > m1:
                    m2, m1, e1 = m1, v, e
                elif v > m2:
                    m2 = v
            cor = pent if e1 else hexa
            if m1 - m2 < 0.0045:  # perto da aresta: a costura
                cor = costura
            l = max(0.0, p[0] * luz[0] + p[1] * luz[1] + p[2] * luz[2])
            k = 0.38 + 0.62 * l
            brilho = max(0.0, l - 0.93) * 6  # o reflexo
            c = tuple(min(255, round(cor[q] * k + 255 * brilho)) for q in range(3))
            px[i, j] = c + (255,)
    # contorno escuro fino
    borda = Image.new("RGBA", (T, T), (0, 0, 0, 0))
    ImageDraw.Draw(borda).ellipse([T / 2 - R, T / 2 - R, T / 2 + R, T / 2 + R], outline=(20, 20, 20, 255), width=max(2, T // 90))
    im.alpha_composite(borda)
    return im.resize((tam, tam), Image.LANCZOS)


def selo_98(tam):
    """O "98" num botao do Windows 98 (cinza com relevo), lado tam."""
    s = 64
    im = Image.new("RGBA", (s, s // 2 + 4), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    w, h = im.size
    d.rectangle([0, 0, w - 1, h - 1], fill=OURO + (255,))
    d.line([(0, 0), (w - 1, 0)], fill=(255, 236, 140, 255), width=2)
    d.line([(0, 0), (0, h - 1)], fill=(255, 236, 140, 255), width=2)
    d.line([(0, h - 1), (w - 1, h - 1)], fill=(150, 110, 0, 255), width=2)
    d.line([(w - 1, 0), (w - 1, h - 1)], fill=(150, 110, 0, 255), width=2)
    f = ImageFont.truetype(FONTE, 30)
    bx = d.textbbox((0, 0), "98", font=f)
    d.text(((w - (bx[2] - bx[0])) / 2 - bx[0], (h - (bx[3] - bx[1])) / 2 - bx[1]), "98", font=f, fill=PRETO + (255,))
    f2 = tam / s
    return im.resize((round(w * f2), round(h * f2)), Image.LANCZOS)


def icone(tam, bola):
    s = 4
    T = tam * s
    im = Image.new("RGBA", (T, T), (0, 0, 0, 0))
    fundo = Image.new("RGBA", (T, T), PETROLEO + (255,))
    # degrade de cima para baixo
    gd = ImageDraw.Draw(fundo)
    for y in range(T):
        t = y / T
        gd.line([(0, y), (T, y)], fill=tuple(round(PETROLEO[i] * (1 - t) + PETROLEO_ESC[i] * t) for i in range(3)) + (255,))
    mask = Image.new("L", (T, T), 0)
    ImageDraw.Draw(mask).rounded_rectangle([0, 0, T - 1, T - 1], radius=T * 0.2, fill=255)
    im.paste(fundo, (0, 0), mask)
    b = bola.resize((round(T * 0.74), round(T * 0.74)), Image.LANCZOS)
    # sombra da bola
    sb = Image.new("RGBA", b.size, (0, 0, 0, 0))
    sb.paste((0, 0, 0, 90), (0, 0), b.split()[3])
    sb = sb.filter(ImageFilter.GaussianBlur(T / 60))
    bx, by = round(T * 0.13), round(T * 0.08)
    im.alpha_composite(sb, (bx + T // 40, by + T // 30))
    im.alpha_composite(b, (bx, by))
    selo = selo_98(round(T * 0.42))
    im.alpha_composite(selo, (T - selo.width - round(T * 0.08), T - selo.height - round(T * 0.08)))
    return im.resize((tam, tam), Image.LANCZOS)


def nome():
    """700x135: REFOOT em branco com contorno e o 98 numa barra de titulo do Windows 98."""
    s = 3
    W, H = 700 * s, 135 * s
    im = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    f = ImageFont.truetype(FONTE, 118 * s)
    borda = 7 * s

    def medir(t):
        b = d.textbbox((0, 0), t, font=f, stroke_width=borda)
        return b, b[2] - b[0]

    (b_re, w_re), (b_ft, w_ft) = medir("RE"), medir("FOOT")
    f98 = ImageFont.truetype(FONTE, 84 * s)
    b98 = d.textbbox((0, 0), "98", font=f98)
    w98 = b98[2] - b98[0] + 34 * s
    total = w_re + w_ft + 10 * s + w98
    x = (W - total) / 2
    alt = b_re[3] - b_re[1]
    y = (H - alt) / 2 - b_re[1]
    d.text((x - b_re[0], y), "RE", font=f, fill=OURO + (255,), stroke_width=borda, stroke_fill=BRANCO + (255,))
    x += w_re - borda
    d.text((x - b_ft[0], y), "FOOT", font=f, fill=VERDE + (255,), stroke_width=borda, stroke_fill=BRANCO + (255,))
    x += w_ft + 10 * s
    # a barra do 98: contorno branco, degrade marinho -> azul (a barra de titulo do Windows 98)
    topo, base = y + b_re[1] + 6 * s, y + b_re[3] - 6 * s
    caixa = [round(x), round(topo), round(x + w98), round(base)]
    d.rounded_rectangle([caixa[0] - borda, caixa[1] - borda, caixa[2] + borda, caixa[3] + borda], radius=12 * s, fill=BRANCO + (255,))
    barra = Image.new("RGBA", (caixa[2] - caixa[0], caixa[3] - caixa[1]))
    bd = ImageDraw.Draw(barra)
    for xx in range(barra.width):
        t = xx / barra.width
        bd.line([(xx, 0), (xx, barra.height)], fill=tuple(round(MARINHO[i] * (1 - t) + AZUL_TITULO[i] * t) for i in range(3)) + (255,))
    m = Image.new("L", barra.size, 0)
    ImageDraw.Draw(m).rounded_rectangle([0, 0, barra.width - 1, barra.height - 1], radius=8 * s, fill=255)
    im.paste(barra, (caixa[0], caixa[1]), m)
    d.text(((caixa[0] + caixa[2]) / 2 - (b98[0] + b98[2]) / 2, (caixa[1] + caixa[3]) / 2 - (b98[1] + b98[3]) / 2),
           "98", font=f98, fill=BRANCO + (255,))
    # recorta o que sobrou e encaixa em 700x135 com margem
    im = im.crop(im.getbbox())
    esc = min((700 - 8) / im.width, (135 - 8) / im.height)
    im = im.resize((round(im.width * esc), round(im.height * esc)), Image.LANCZOS)
    final = Image.new("RGBA", (700, 135), (0, 0, 0, 0))
    final.alpha_composite(im, ((700 - im.width) // 2, (135 - im.height) // 2))
    return final


def capa(w, h, bola):
    im = Image.new("RGB", (w, h), VERDE_ESC)
    d = ImageDraw.Draw(im)
    # faixas do gramado
    faixa = w // 12
    for i in range(0, w, faixa * 2):
        d.rectangle([i, 0, i + faixa - 1, h], fill=(14, 96, 46))
    # vinheta: escurece as bordas
    v = Image.new("L", (w, h), 0)
    ImageDraw.Draw(v).ellipse([-w * 0.25, -h * 0.35, w * 1.25, h * 1.35], fill=255)
    v = v.filter(ImageFilter.GaussianBlur(h / 6))
    im = Image.composite(im, Image.new("RGB", (w, h), (4, 40, 20)), v)
    d = ImageDraw.Draw(im)
    # o circulo central e a linha do meio
    lc = (60, 150, 90)
    cx, cy, r = w / 2, h / 2, h * 0.36
    d.line([(cx, 0), (cx, h)], fill=lc, width=4)
    d.ellipse([cx - r, cy - r, cx + r, cy + r], outline=lc, width=4)
    b = bola.resize((round(h * 0.62), round(h * 0.62)), Image.LANCZOS)
    sb = Image.new("RGBA", b.size, (0, 0, 0, 0))
    sb.paste((0, 0, 0, 110), (0, 0), b.split()[3])
    sb = sb.filter(ImageFilter.GaussianBlur(8))
    im = im.convert("RGBA")
    im.alpha_composite(sb, (round(cx - b.width / 2) + 10, round(cy - b.height / 2) + 14))
    im.alpha_composite(b, (round(cx - b.width / 2), round(cy - b.height / 2)))
    return im.convert("RGB")


def main():
    bola = bola_3d(512, VERDE_ESC, OURO, (8, 60, 28), giro=(0.25, -0.35))
    bola.resize((256, 256), Image.LANCZOS).save(os.path.join(MARCA, "bola-256.png"))
    bola.resize((256, 256), Image.LANCZOS).save(os.path.join(ANDROID, "Resources", "drawable", "splash_bola.png"))
    icone(256, bola).save(os.path.join(MARCA, "icone-256.png"))
    icone(512, bola).save(os.path.join(MARCA, "icone-512.png"))
    icone(512, bola).save(os.path.join(ANDROID, "Icon.png"))
    nome().save(os.path.join(MARCA, "nome.png"))
    capa(1076, 605, bola).save(os.path.join(MARCA, "capa-16x9.jpg"), quality=92)
    capa(807, 605, bola).save(os.path.join(MARCA, "capa-4x3.jpg"), quality=92)


if __name__ == "__main__":
    main()
