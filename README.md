# JBL Tray Connector 🎧

Um aplicativo leve para a bandeja do Windows (System Tray) para conectar e desconectar rapidamente seus dispositivos de áudio Bluetooth JBL (ou qualquer outro dispositivo pareado) com apenas um clique.

## ✨ Funcionalidades

- **Clique Esquerdo Direto**: Conecta ou desconeta o dispositivo principal instantaneamente.
- **Interface Otimista**: O ícone da bandeja muda o status na hora do clique, sem esperar o hardware responder.
- **Feedback Visual (Spinner)**: Animação de carregamento no ícone da bandeja enquanto a conexão está sendo processada.
- **Status em Tempo Real**: Ícone verde (conectado) e cinza (desconectado) que se atualiza automaticamente em segundo plano (Background Worker).
- **Detecção de "Conexão Fantasma"**: Monitoramento via WMI e PnP Manager para detectar e forçar a limpeza de status quando o Windows trava em "Conectado".
- **Baixo Consumo**: Utiliza APIs Win32 (Bluetooth Classic) para buscas instantâneas sem travamentos na interface.

## 🛠️ Requisitos

- **Windows 10/11**
- **.NET 10.0** (SDK para compilar ou Runtime para rodar)
- Bluetooth ativado e dispositivos já pareados.

## 🚀 Como usar

1.  Clone o repositório ou baixe o executável.
2.  Execute o `Devn.TrayUsbDeviceControl.exe`.
3.  **Botão Esquerdo**: Alterna a conexão do seu dispositivo JBL.
4.  **Botão Direito**: Abre o menu com a lista completa, opção de atualização e ferramentas de diagnóstico.

## 🏗️ Estrutura do Projeto

- `Program.cs`: Lógica principal da aplicação de bandeja e do monitor de background.
- `BluetoothDeviceService.cs`: Serviço de descoberta e verificação de status (Win32 + WMI).
- `DeviceConnectionService.cs`: Gerenciador de conexão/desconexão via `bthprops.cpl`.
- `BluetoothClassicService.cs`: Wrapper para APIs Win32 de Bluetooth.

---
Desenvolvido para simplificar o controle de áudio Bluetooth no dia a dia.
