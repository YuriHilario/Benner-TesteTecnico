# MicroOndas

Este projeto simula um Micro-Ondas Digital interativo, desenvolvido em ASP.NET Core e Razor Pages, utilizando SignalR para atualizações em tempo real do timer e Dapper para persistência de programas pré-definidos no SQL Server.

**Recursos Principais**
Aquecimento Personalizado: Definição manual de tempo e potência.
Programas Pré-Definidos: Carregamento de programas do SQL Server.
Controles: Iniciar, Pausar, Continuar e Cancelar o aquecimento.
Tempo Real: Atualização imediata do display via SignalR (MicroOndasTimerService).
Persistência: Uso do Dapper no SqlHeatingProgramRepository para acesso ao banco de dados.

**Requisitos de Desenvolvimento**
Para executar e desenvolver o projeto localmente, você precisará ter instalado:
.NET 8 SDK ou superior
Docker Desktop (Necessário para executar o container do SQL Server)
SQL Server Management Studio (SSMS) ou Azure Data Studio (Opcional, mas altamente recomendado para gerenciar o banco de dados).

>  This is a challenge by [Coodesh](https://coodesh.com/)

**Inicialização do Projeto**
Configurar o Banco de Dados (SQL Server via Docker)
O projeto está configurado para usar um container Docker como servidor SQL.

**A. Iniciar o Container**:
Navegue até a pasta que contém o docker-compose.yml e utilize o comando para subir o container SQL Server:

**B. Criar o Schema e Seed (Manual)**
Conecte-se ao servidor SQL (localhost,1433 ou sqlserver) usando o login a senha (conforme definido no appsettings.json e no comando Docker).

Execute os scripts a seguir para criar a tabela e inserir os programas pré-definidos:
-- Criação do Banco
CREATE DATABASE MicroOndasDB;

-- Criação da Tabela de Programas
CREATE TABLE HeatingPrograms (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Food NVARCHAR(255) NULL, -- CORREÇÃO: Substitui 'Description' por 'Food' para alinhar com a entidade C#
    TimeInSeconds INT NOT NULL,
    Power INT NOT NULL,
    HeatingChar CHAR(1) NOT NULL,
    Instructions NVARCHAR(500) NOT NULL,
    IsPredefined BIT NOT NULL DEFAULT 1, -- CORREÇÃO: Adicionada coluna 'IsPredefined' (Default 1 para programas pré-definidos)
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Inserção de Programas Iniciais
INSERT INTO HeatingPrograms
     (Name, Food, TimeInSeconds, Power, HeatingChar, Instructions, IsPredefined)
VALUES
     ('Pipoca', 'Prepara porções médias de pipoca.', 120, 8, '*', 'Observe o intervalo entre os estouros. Não use pacotes rasgados.', 1),
     ('Leite', 'Aquecimento de leite para bebidas.', 90, 5, '~', 'Cuidado para não ferver. Use um recipiente grande.', 1),
     ('Carne', 'Descongelamento e aquecimento de carnes.', 180, 10, '#', 'Vire na metade do tempo para aquecimento uniforme.', 1),
     ('Arroz', 'Reaquecimento de porções de arroz cozido.', 300, 7, '+', 'Mexa ao final do aquecimento. Adicione um pouco de água.', 1);
