Reddit Analyzer
Инструкция запуска
Убедитесь что Docker Desktop запущен и работает
Склонируйте репозиторий и перейдите в папку проекта:
cd RedditAnalyzer/RedditAnalyzer
Соберите Docker образ:
docker build -t reddit-analyzer .
Запустите контейнер:
docker run -p 5000:8080 reddit-analyzer
Откройте в браузере:
http://localhost:5000/swagger
Пример запроса
POST http://localhost:5000/api/reddit
json{
  "items": [
    {
      "subreddit": "r/news",
      "keywords": ["war", "president"]
    },
    {
      "subreddit": "r/aww",
      "keywords": ["cat", "dog"]
    }
  ],
  "limit": 25
}
Пример ответа:
json{
  "/r/news": [
    "UK 'weeks away' from medicine shortages if Iran war continues, experts say",
    "French police thwart a suspected bombing outside a Bank of America building in Paris",
    "EU urges countries to start filling gas storage early amid Iran war, sources say"
  ],
  "/r/aww": [
    "My office has an open dog policy",
    "Random street cat cuddles are undefeated",
    "My cat thinks I am a pillow.",
    "He was lying there like dog",
    "Let me introduce my cat doing wrist exercises"
  ]
}
Описание использования
Сервис принимает список сабреддитов и ключевых слов. Для каждого сабреддита загружаются первые N постов, после чего фильтруются по ключевым словам в заголовке и тексте поста. Результат возвращается в виде словаря где ключ это название сабреддита а значение это список подходящих заголовков. Все запросы логируются в файл out.log.
Теоретический вопрос
Какие проблемы могут возникнуть при получении данных через HTTP и парсинг HTML и как их решать?
Проблема 1. Хрупкость структуры. HTML страницы меняются при любом обновлении сайта, после чего парсер перестаёт работать и требует доработки.
Проблема 2. Блокировки. Сайты определяют автоматические запросы по User-Agent, частоте запросов или IP адресу и блокируют их.
Проблема 3. Производительность. HTML страницы содержат много лишних данных, их загрузка и парсинг занимают больше времени чем работа с API.
Проблема 4. Юридические ограничения. Автоматический сбор данных может нарушать правила использования сайта.
Решение. Вместо парсинга HTML использовать официальный Reddit JSON API по адресу reddit.com/r/название.json. Он стабилен, возвращает только нужные данные в структурированном виде и официально поддерживается Reddit.
