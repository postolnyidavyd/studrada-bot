namespace studrada_bot.Data.Enums;

public enum PostStatus {
    Open,              // вхід А: завдання в групі, ніхто не взяв
    Claimed,           // власник є, робить контент
    Pending,           // контент є, чекає аппрув
    ChangesRequested,  // відхилено з причиною, повернуто власнику
    Scheduled,         // аппрувнуто, чекає часу публікації (Hangfire)
    AwaitingHumanPost, // PublishMode=Human: чекає, поки людина запостить вручну й підтвердить
    Published,         // бот опублікував
    Done,              // PublishMode=Human: людина запостила сама й підтвердила
    Cancelled          // відмова / скасовано, без повернення
}