using System.Collections.Generic;

namespace NorthernTown2026
{
    /// <summary>平行世界 2026 北方小镇 · 轻科幻分支剧情表。</summary>
    public static class NorthernTownStoryContent
    {
        public static Dictionary<string, StoryNode> BuildGraph()
        {
            var g = new Dictionary<string, StoryNode>();

            g["start"] = new StoryNode
            {
                Id = "start",
                Text = "2026 年，冬末。华北一座收缩型小镇，旧国营厂的红砖墙外排着新漆的「街道算力服务站」。你攥着失业第三个月的排号条，手机震了一下——没有署名，只有一行坐标与词：「低轨信标·今夜可见。」\n\n风里有融雪和铁锈味。你要先做什么？",
                Choices =
                {
                    new ChoiceOption { Text = "去算力服务站排队，看这条短信是不是恶作剧。", NextNodeId = "station_queue", GrantXp = 15 },
                    new ChoiceOption { Text = "绕到后巷，帮那个搬纸箱的老人一把。", NextNodeId = "old_man_intro", GrantXp = 10 },
                    new ChoiceOption { Text = "在街角抽烟发呆，观察骑手与灰色背包的交易。", NextNodeId = "courier_meet", GrantXp = 10 }
                }
            };

            g["station_queue"] = new StoryNode
            {
                Id = "station_queue",
                Text = "队伍里有人抱怨「社区算法又改了我的买菜券」。玻璃门内，蓝光一排排闪烁。你的号码还有七位。窗外，旧厂烟囱像熄灭的蜡烛。\n\n你注意到侧门没关严。",
                Choices =
                {
                    new ChoiceOption { Text = "从侧门溜进维护通道。", NextNodeId = "station_enter", GrantXp = 20 },
                    new ChoiceOption { Text = "老老实实继续排队。", NextNodeId = "station_peak_sms", GrantXp = 5 }
                }
            };

            g["station_peak_sms"] = new StoryNode
            {
                Id = "station_peak_sms",
                Text = "你坐了一小时。短信又来了：「抬头。」你抬头，云层裂开一线冷白——像有人用手电筒从天上扫过小镇。周围的人毫无反应，像被滤掉了频段。\n\n你手心出汗。",
                Choices =
                {
                    new ChoiceOption { Text = "把这一幕拍下来发本地论坛。", NextNodeId = "hub_after_day", GrantXp = 25 },
                    new ChoiceOption { Text = "离开队伍，去后巷冷静一下。", NextNodeId = "old_man_intro", GrantXp = 10 }
                }
            };

            g["station_enter"] = new StoryNode
            {
                Id = "station_enter",
                Text = "维护通道里弥漫着冷却液和消毒水味。墙上有手写：「记忆缓存非医疗用途勿动」。一台老终端亮着，提示输入维护口令。",
                Choices =
                {
                    new ChoiceOption
                    {
                        Text = "尝试破解终端（机巧检定：属性+1d10 ≥ 12）",
                        Check = new StatCheck
                        {
                            Stat = StatId.机巧,
                            Threshold = 12,
                            SuccessNodeId = "station_reveal_success",
                            FailNodeId = "station_reveal_fail"
                        }
                    },
                    new ChoiceOption { Text = "撤退，改去街角观察骑手。", NextNodeId = "courier_meet", GrantXp = 5 }
                }
            };

            g["station_reveal_success"] = new StoryNode
            {
                Id = "station_reveal_success",
                Text = "光标跳动三下，你进去了。日志里写着温柔的暴行：「邻里纠纷关键词·温和降级」「悲伤事件·延迟推送」。小镇的疼被算法抹成可管理的噪声。你胃里发冷。\n\n你在抽屉里摸到一只改装旧手机（离线天线）。",
                Choices =
                {
                    new ChoiceOption
                    {
                        Text = "带走手机，去广场想想怎么处理这些日志。",
                        NextNodeId = "hub_after_day",
                        GrantItemId = "gear_old_phone",
                        GrantXp = 40
                    },
                    new ChoiceOption { Text = "当场把日志复制到公开缓存（冒险）。", NextNodeId = "ending_public", GrantXp = 30 }
                }
            };

            g["station_reveal_fail"] = new StoryNode
            {
                Id = "station_reveal_fail",
                Text = "终端弹出红色「权限不足」。走廊尽头传来脚步声。你迅速拔线，从侧门溜出，心脏撞着肋骨。至少你确认了一件事：有人在编辑小镇的记忆。",
                Choices =
                {
                    new ChoiceOption { Text = "去后巷喘口气，顺便看看老人。", NextNodeId = "old_man_intro", GrantXp = 15 },
                    new ChoiceOption { Text = "找骑手打听灰色数据的去向。", NextNodeId = "courier_meet", GrantXp = 15 }
                }
            };

            g["old_man_intro"] = new StoryNode
            {
                Id = "old_man_intro",
                Text = "后巷里，老人把纸箱往上摞，纸边割手。他抬头笑：「年轻人，帮把手？我给你讲个六十年代的故事。」他的眼镜反光，像两块不新鲜的冰。",
                Choices =
                {
                    new ChoiceOption
                    {
                        Text = "帮他扛到二楼（体魄检定：≥11）",
                        Check = new StatCheck
                        {
                            Stat = StatId.体魄,
                            Threshold = 11,
                            SuccessNodeId = "old_man_chat",
                            FailNodeId = "old_man_tired"
                        }
                    },
                    new ChoiceOption { Text = "借口有事离开。", NextNodeId = "hub_after_day", GrantXp = 5 }
                }
            };

            g["old_man_tired"] = new StoryNode
            {
                Id = "old_man_tired",
                Text = "你抬到一半，腰像要断。老人拍拍你：「行了，别硬撑。」他塞给你一枚铜铃铛：「挂包上，夜里走路响一点，安全。」",
                Choices =
                {
                    new ChoiceOption { Text = "收下铃铛。", NextNodeId = "old_man_chat", GrantItemId = "gear_charm", GrantXp = 15 },
                    new ChoiceOption { Text = "推辞，去广场。", NextNodeId = "hub_after_day", GrantXp = 5 }
                }
            };

            g["old_man_chat"] = new StoryNode
            {
                Id = "old_man_chat",
                Text = "楼上，老人泡茶。他说话时，句子中间有半秒「空拍」——不像呼吸，更像缓冲。你突然意识到：他可能不是「完全在场」。",
                Choices =
                {
                    new ChoiceOption
                    {
                        Text = "旁敲侧击，观察微表情（洞察检定：≥12）",
                        Check = new StatCheck
                        {
                            Stat = StatId.洞察,
                            Threshold = 12,
                            SuccessNodeId = "old_man_secret",
                            FailNodeId = "old_man_smalltalk"
                        }
                    },
                    new ChoiceOption { Text = "礼貌听完故事，告辞。", NextNodeId = "hub_after_day", GrantXp = 20 }
                }
            };

            g["old_man_smalltalk"] = new StoryNode
            {
                Id = "old_man_smalltalk",
                Text = "你只觉得老人唠叨。他送你到楼梯口：「下雪天路滑。」你心里那点异样被茶热盖过去了。",
                Choices =
                {
                    new ChoiceOption { Text = "去广场汇合今天的线索。", NextNodeId = "hub_after_day", GrantXp = 15 }
                }
            };

            g["old_man_secret"] = new StoryNode
            {
                Id = "old_man_secret",
                Text = "你问：「您上次去医院是哪天？」老人停顿，笑：「昨天啊。」可窗外海报写着上周停诊。他低声：「我是备份。真的那个……在城里透析。」\n\n你后背发凉，也第一次同情算法：它要模拟多少温柔，才撑得住这样的小镇。",
                Choices =
                {
                    new ChoiceOption { Text = "保守秘密，去处理你自己的信标短信。", NextNodeId = "hub_after_day", GrantXp = 35 },
                    new ChoiceOption
                    {
                        Text = "若已拿到离线密钥碎片，可尝试唤醒公共缓存里的备份对话。",
                        NextNodeId = "ending_hidden",
                        RequiresItemId = "fragment_key",
                        GrantXp = 20
                    }
                }
            };

            g["courier_meet"] = new StoryNode
            {
                Id = "courier_meet",
                Text = "骑手把头盔一掀：「看什么？送货。」他背包侧袋露出半截接口标签：「离线密钥·勿连公网」。买家在暗处招手。",
                Choices =
                {
                    new ChoiceOption
                    {
                        Text = "压着火气跟他谈价（镇定检定：≥12）",
                        Check = new StatCheck
                        {
                            Stat = StatId.镇定,
                            Threshold = 12,
                            SuccessNodeId = "courier_key",
                            FailNodeId = "courier_fail"
                        }
                    },
                    new ChoiceOption { Text = "报警。", NextNodeId = "ending_arrest", GrantXp = 10 }
                }
            };

            g["courier_fail"] = new StoryNode
            {
                Id = "courier_fail",
                Text = "你声音发紧，对方冷笑：「学生吧？」交易散了。你只捡到一件被丢下的厂牌工装外套，内衬还留着机台的机油味。",
                Choices =
                {
                    new ChoiceOption { Text = "穿上外套，去广场整理思路。", NextNodeId = "hub_after_day", GrantItemId = "gear_worker_coat", GrantXp = 15 }
                }
            };

            g["courier_key"] = new StoryNode
            {
                Id = "courier_key",
                Text = "你把钱数清，他丢给你一枚冷硬的碎片：「别连 Wi‑Fi 试。」那是「离线密钥碎片」，像半片贝壳。",
                Choices =
                {
                    new ChoiceOption { Text = "收好碎片，离开巷口。", NextNodeId = "hub_after_day", GrantItemId = "fragment_key", GrantXp = 35 }
                }
            };

            g["hub_after_day"] = new StoryNode
            {
                Id = "hub_after_day",
                Text = "黄昏把烟囱染成紫红。广场大屏滚动：「本市记忆缓存优化·为您更安心」。你想起信标、备份老人、温柔篡改的日志。\n\n你站在风口，必须选一条路把今天落地。",
                Choices =
                {
                    new ChoiceOption
                    {
                        Text = "带着碎片与证据离开小镇，去更大的城市找律师与记者。",
                        NextNodeId = "ending_leave",
                        RequiresItemId = "fragment_key",
                        GrantXp = 30
                    },
                    new ChoiceOption
                    {
                        Text = "用机巧与洞察硬闯公开缓存，把日志写进广场大屏（需洞察+机巧总和≥10）。",
                        NextNodeId = "ending_public",
                        RequiresInsightSum = 10,
                        GrantXp = 20
                    },
                    new ChoiceOption { Text = "算了，回家睡觉，当一切是冬末幻觉。", NextNodeId = "ending_leave_soft", GrantXp = 10 },
                    new ChoiceOption
                    {
                        Text = "拐进报刊亭，翻一版三年前的厂报，核对时间线。",
                        NextNodeId = "newsstand_kiosk",
                        GrantXp = 0
                    }
                }
            };

            g["newsstand_kiosk"] = new StoryNode
            {
                Id = "newsstand_kiosk",
                Text =
                    "玻璃柜台里，油墨褪成灰茶色。头版写着「稳岗增产」，落款日期却比你记忆里那条停产公告晚半年。老板从后面打盹里抬头：「旧报纸不卖，只能看。」\n\n你把折痕对齐，像对齐两条分叉的记忆。",
                Choices =
                {
                    new ChoiceOption
                    {
                        Text = "把报纸折好，带回广场继续想今天的决定。",
                        NextNodeId = "hub_after_day",
                        GrantXp = 15
                    }
                }
            };

            g["ending_leave"] = new StoryNode
            {
                Id = "ending_leave",
                Text = "结局：「北上的信标」\n\n你把碎片塞进内衣口袋，像藏一颗不会联网的心脏。火车开动时，你看见小镇灯光像一块被刮花的屏幕。你知道这不是胜利，只是下一步的开始。",
                Choices = { new ChoiceOption { Text = "（再玩一次，寻找其他结局）", NextNodeId = "start" } }
            };

            g["ending_leave_soft"] = new StoryNode
            {
                Id = "ending_leave_soft",
                Text = "结局：「雪落在缓存上」\n\n你缩进被子。夜里手机仍偶尔亮起无署名提示。你没有输，也没有赢——你只是暂时从算法的温柔里下班。",
                Choices = { new ChoiceOption { Text = "（再玩一次，寻找其他结局）", NextNodeId = "start" } }
            };

            g["ending_public"] = new StoryNode
            {
                Id = "ending_public",
                Text = "结局：「广场一分钟」\n\n大屏卡顿，像有人咳嗽。几行日志掠过：缓存修改、延迟悲伤、备份替身。人群抬头，第一次用真人的嘈杂盖过算法的静音。警察哨声与快门声同时响起——你笑了一下，把外套帽子拉低。",
                Choices = { new ChoiceOption { Text = "（再玩一次，寻找其他结局）", NextNodeId = "start" } }
            };

            g["ending_hidden"] = new StoryNode
            {
                Id = "ending_hidden",
                Text = "结局：「备份的对话」\n\n你把碎片贴近老人家的旧收音机，噪声里浮出两段对话交叠。备份与真身同时咳嗽。算法在门外停了三秒——像学会了迟疑。",
                Choices = { new ChoiceOption { Text = "（再玩一次，寻找其他结局）", NextNodeId = "start" } }
            };

            g["ending_arrest"] = new StoryNode
            {
                Id = "ending_arrest",
                Text = "结局：「热心市民」\n\n警车来得很及时。你做笔录时，骑手早已消失在路由之外。小镇继续温柔。",
                Choices = { new ChoiceOption { Text = "（再玩一次，寻找其他结局）", NextNodeId = "start" } }
            };

            return g;
        }
    }
}
